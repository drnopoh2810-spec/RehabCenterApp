using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using RehabCenterApp.Services;
using RehabCenterApp.Models;

namespace RehabCenterApp.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;

    private int _beneficiariesCount;
    public int BeneficiariesCount
    {
        get => _beneficiariesCount;
        set => this.RaiseAndSetIfChanged(ref _beneficiariesCount, value);
    }

    private int _todaySessionsCount;
    public int TodaySessionsCount
    {
        get => _todaySessionsCount;
        set => this.RaiseAndSetIfChanged(ref _todaySessionsCount, value);
    }

    private decimal _monthRevenue;
    public decimal MonthRevenue
    {
        get => _monthRevenue;
        set => this.RaiseAndSetIfChanged(ref _monthRevenue, value);
    }

    private decimal _monthExpenses;
    public decimal MonthExpenses
    {
        get => _monthExpenses;
        set => this.RaiseAndSetIfChanged(ref _monthExpenses, value);
    }

    public ObservableCollection<Reminder> UpcomingReminders { get; } = new();
    public ObservableCollection<Session> TodaySessions { get; } = new();

    // Charts
    public ISeries[] MonthlyBeneficiariesSeries { get; set; } = Array.Empty<ISeries>();
    public ISeries[] RevenueExpensesSeries { get; set; } = Array.Empty<ISeries>();
    public ISeries[] DisabilityDistributionSeries { get; set; } = Array.Empty<ISeries>();

    public Axis[] XAxes { get; set; } = new[]
    {
        new Axis { Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" } }
    };

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    public DashboardViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var stats = await _dbService.GetDashboardStatsAsync();
        BeneficiariesCount = stats.beneficiaries;
        TodaySessionsCount = stats.sessions;
        MonthRevenue = stats.revenue;
        MonthExpenses = stats.expenses;

        var reminders = await _dbService.GetUpcomingRemindersAsync(7);
        UpcomingReminders.Clear();
        foreach (var r in reminders)
            UpcomingReminders.Add(r);

        var sessions = await _dbService.GetTodaySessionsAsync();
        TodaySessions.Clear();
        foreach (var s in sessions)
            TodaySessions.Add(s);

        // Initialize charts with sample data (replace with real data)
        MonthlyBeneficiariesSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Beneficiaries",
                Values = new[] { 5, 8, 12, 15, 18, 20 },
                Fill = new SolidColorPaint(SKColor.Parse("#1e3a5f"))
            }
        };

        RevenueExpensesSeries = new ISeries[]
        {
            new LineSeries<decimal>
            {
                Name = "Revenue",
                Values = new[] { 1000m, 1500m, 2000m, 1800m, 2500m, 3000m },
                Stroke = new SolidColorPaint(SKColor.Parse("#48bb78")) { StrokeThickness = 3 }
            },
            new LineSeries<decimal>
            {
                Name = "Expenses",
                Values = new[] { 800m, 900m, 1200m, 1100m, 1500m, 1800m },
                Stroke = new SolidColorPaint(SKColor.Parse("#e53e3e")) { StrokeThickness = 3 }
            }
        };

        DisabilityDistributionSeries = new ISeries[]
        {
            new PieSeries<int> { Name = "Physical", Values = new[] { 30 }, Fill = new SolidColorPaint(SKColor.Parse("#1e3a5f")) },
            new PieSeries<int> { Name = "Mental", Values = new[] { 25 }, Fill = new SolidColorPaint(SKColor.Parse("#2c5282")) },
            new PieSeries<int> { Name = "Sensory", Values = new[] { 20 }, Fill = new SolidColorPaint(SKColor.Parse("#48bb78")) },
            new PieSeries<int> { Name = "Other", Values = new[] { 15 }, Fill = new SolidColorPaint(SKColor.Parse("#ed8936")) }
        };

        this.RaisePropertyChanged(nameof(MonthlyBeneficiariesSeries));
        this.RaisePropertyChanged(nameof(RevenueExpensesSeries));
        this.RaisePropertyChanged(nameof(DisabilityDistributionSeries));
    }
}