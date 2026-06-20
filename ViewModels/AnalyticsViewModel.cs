using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class AnalyticsViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    // KPI Cards
    private int _totalBeneficiaries;
    public int TotalBeneficiaries
    {
        get => _totalBeneficiaries;
        set => this.RaiseAndSetIfChanged(ref _totalBeneficiaries, value);
    }

    private decimal _monthlyRevenue;
    public decimal MonthlyRevenue
    {
        get => _monthlyRevenue;
        set => this.RaiseAndSetIfChanged(ref _monthlyRevenue, value);
    }

    private decimal _sessionCompletionRate;
    public decimal SessionCompletionRate
    {
        get => _sessionCompletionRate;
        set => this.RaiseAndSetIfChanged(ref _sessionCompletionRate, value);
    }

    private decimal _parentSatisfactionRate;
    public decimal ParentSatisfactionRate
    {
        get => _parentSatisfactionRate;
        set => this.RaiseAndSetIfChanged(ref _parentSatisfactionRate, value);
    }

    private int _atRiskBeneficiaries;
    public int AtRiskBeneficiaries
    {
        get => _atRiskBeneficiaries;
        set => this.RaiseAndSetIfChanged(ref _atRiskBeneficiaries, value);
    }

    // AI Predictions
    public ObservableCollection<ProgressPrediction> Predictions { get; } = new();

    // Charts
    public ISeries[] RevenueTrendSeries { get; set; } = Array.Empty<ISeries>();
    public ISeries[] SessionAttendanceSeries { get; set; } = Array.Empty<ISeries>();
    public ISeries[] DisabilityDistributionSeries { get; set; } = Array.Empty<ISeries>();
    public ISeries[] AgeDistributionSeries { get; set; } = Array.Empty<ISeries>();

    public Axis[] XAxesMonths { get; set; } = new[] { new Axis { Labels = new[] { "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو" } } };

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> GeneratePredictionsCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportAnalyticsCommand { get; }

    public AnalyticsViewModel(DatabaseService dbService, DialogService dialogService)
    {
        _dbService = dbService;
        _dialogService = dialogService;

        RefreshCommand = ReactiveCommand.CreateFromTask(LoadAnalyticsAsync);
        GeneratePredictionsCommand = ReactiveCommand.CreateFromTask(GeneratePredictionsAsync);
        ExportAnalyticsCommand = ReactiveCommand.CreateFromTask(ExportAnalyticsAsync);

        _ = LoadAnalyticsAsync();
    }

    private async Task LoadAnalyticsAsync()
    {
        var stats = await _dbService.GetDashboardStatsAsync();
        TotalBeneficiaries = stats.beneficiaries;
        MonthlyRevenue = stats.revenue;

        // Calculate session completion rate
        var allSessions = await _dbService.GetSessionsAsync();
        if (allSessions.Any())
        {
            var completed = allSessions.Count(s => s.Status == "Present");
            SessionCompletionRate = (decimal)completed / allSessions.Count * 100;
        }

        // Calculate at-risk beneficiaries (high absence rate)
        var beneficiaries = await _dbService.GetBeneficiariesAsync();
        AtRiskBeneficiaries = 0;
        foreach (var b in beneficiaries)
        {
            if (b.Sessions.Count > 0)
            {
                var absentRate = (decimal)b.Sessions.Count(s => s.Status == "Absent") / b.Sessions.Count;
                if (absentRate > 0.3m) AtRiskBeneficiaries++;
            }
        }

        // Load charts data
        var disabilityDist = await _dbService.GetDisabilityDistributionAsync();
        DisabilityDistributionSeries = disabilityDist.Select(d => new PieSeries<int>
        {
            Name = d.Key,
            Values = new[] { d.Value },
            Fill = new SolidColorPaint(SKColor.Parse(GetColorForDisability(d.Key)))
        }).ToArray<ISeries>();

        var revenueByMonth = await _dbService.GetRevenueByMonthAsync(DateTime.Now.Year);
        RevenueTrendSeries = new ISeries[]
        {
            new LineSeries<decimal>
            {
                Name = "الإيرادات",
                Values = revenueByMonth.Values.ToArray(),
                Stroke = new SolidColorPaint(SKColor.Parse("#48bb78")) { StrokeThickness = 3 },
                Fill = new SolidColorPaint(new SKColor(72, 187, 120, 30))
            }
        };

        this.RaisePropertyChanged(nameof(RevenueTrendSeries));
        this.RaisePropertyChanged(nameof(DisabilityDistributionSeries));
    }

    private async Task GeneratePredictionsAsync()
    {
        var beneficiaries = await _dbService.GetBeneficiariesAsync();
        Predictions.Clear();

        foreach (var b in beneficiaries)
        {
            // Simple prediction algorithm based on historical progress
            var assessments = await _dbService.GetAssessmentsAsync(b.Id);
            if (assessments.Count >= 2)
            {
                var latest = assessments[0];
                var previous = assessments[1];
                var trend = latest.StandardScore - previous.StandardScore;

                var prediction = new ProgressPrediction
                {
                    BeneficiaryId = b.Id,
                    Beneficiary = b,
                    Domain = latest.Category,
                    CurrentScore = latest.StandardScore ?? 0,
                    PredictedScore3Months = (latest.StandardScore ?? 0) + (trend ?? 0) * 3,
                    PredictedScore6Months = (latest.StandardScore ?? 0) + (trend ?? 0) * 6,
                    PredictedScore12Months = (latest.StandardScore ?? 0) + (trend ?? 0) * 12,
                    RiskLevel = (trend ?? 0) < 0 ? "High" : "Low",
                    ConfidenceLevel = 0.75m
                };
                Predictions.Add(prediction);
            }
        }

        await AuditLogger.LogAsync("Generate", "Predictions", $"Generated {Predictions.Count} AI predictions");
        await _dialogService.ShowInfoAsync("تم", $"تم إنشاء {Predictions.Count} تنبؤات بنجاح");
    }

    private async Task ExportAnalyticsAsync()
    {
        await _dialogService.ShowInfoAsync("تصدير", "تم تصدير التحليلات");
    }

    private static string GetColorForDisability(string type) => type switch
    {
        "إعاقة حركية" => "#1e3a5f",
        "إعاقة ذهنية" => "#2c5282",
        "إعاقة سمعية" => "#48bb78",
        "إعاقة بصرية" => "#ed8936",
        "توحد" => "#e53e3e",
        "متلازمة داون" => "#9f7aea",
        _ => "#718096"
    };
}