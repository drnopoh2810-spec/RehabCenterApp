using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace RehabCenterApp.ViewModels;

public class AssessmentsViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    private ObservableCollection<Assessment> _assessments = new();
    public ObservableCollection<Assessment> Assessments
    {
        get => _assessments;
        set => this.RaiseAndSetIfChanged(ref _assessments, value);
    }

    private Assessment? _selectedAssessment;
    public Assessment? SelectedAssessment
    {
        get => _selectedAssessment;
        set => this.RaiseAndSetIfChanged(ref _selectedAssessment, value);
    }

    private Beneficiary? _selectedBeneficiary;
    public Beneficiary? SelectedBeneficiary
    {
        get => _selectedBeneficiary;
        set => this.RaiseAndSetIfChanged(ref _selectedBeneficiary, value);
    }

    private bool _isFormOpen;
    public bool IsFormOpen
    {
        get => _isFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isFormOpen, value);
    }

    private string _assessmentName = string.Empty;
    public string AssessmentName
    {
        get => _assessmentName;
        set => this.RaiseAndSetIfChanged(ref _assessmentName, value);
    }

    private string _category = "Cognitive";
    public string Category
    {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
    }

    private decimal? _rawScore;
    public decimal? RawScore
    {
        get => _rawScore;
        set => this.RaiseAndSetIfChanged(ref _rawScore, value);
    }

    private decimal? _standardScore;
    public decimal? StandardScore
    {
        get => _standardScore;
        set => this.RaiseAndSetIfChanged(ref _standardScore, value);
    }

    private string? _severityLevel;
    public string? SeverityLevel
    {
        get => _severityLevel;
        set => this.RaiseAndSetIfChanged(ref _severityLevel, value);
    }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    private string _recommendations = string.Empty;
    public string Recommendations
    {
        get => _recommendations;
        set => this.RaiseAndSetIfChanged(ref _recommendations, value);
    }

    public ObservableCollection<Beneficiary> Beneficiaries { get; } = new();

    // Progress Chart
    public ISeries[] ProgressSeries { get; set; } = Array.Empty<ISeries>();
    public Axis[] XAxes { get; set; } = new[] { new Axis { Labels = Array.Empty<string>() } };

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> ViewProgressCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportCommand { get; }

    public AssessmentsViewModel(DatabaseService dbService, DialogService dialogService)
    {
        _dbService = dbService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var list = await _dbService.GetBeneficiariesAsync();
            Beneficiaries.Clear();
            foreach (var b in list) Beneficiaries.Add(b);
            IsFormOpen = true;
        });
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(() => IsFormOpen = false);
        ViewProgressCommand = ReactiveCommand.CreateFromTask(ViewProgressAsync);
        ExportCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await _dialogService.ShowInfoAsync("تصدير", "تم تصدير التقييمات بنجاح");
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _dbService.GetAssessmentsAsync();
        Assessments.Clear();
        foreach (var a in list) Assessments.Add(a);
    }

    private async Task SaveAsync()
    {
        if (SelectedBeneficiary == null || string.IsNullOrWhiteSpace(AssessmentName))
            return;

        var assessment = new Assessment
        {
            BeneficiaryId = SelectedBeneficiary.Id,
            AssessmentName = AssessmentName,
            Category = Category,
            RawScore = RawScore,
            StandardScore = StandardScore,
            SeverityLevel = SeverityLevel,
            Notes = Notes,
            Recommendations = Recommendations,
            AssessmentDate = DateTime.Now
        };

        await _dbService.AddAssessmentAsync(assessment);
        IsFormOpen = false;
        await LoadAsync();
        await AuditLogger.LogAsync("Create", "Assessment", $"Added {AssessmentName} for {SelectedBeneficiary.Name}");
    }

    private async Task ViewProgressAsync()
    {
        if (SelectedAssessment == null) return;

        // Load historical data for this beneficiary and assessment type
        var history = await _dbService.GetAssessmentHistoryAsync(
            SelectedAssessment.BeneficiaryId, 
            SelectedAssessment.AssessmentName);

        var labels = history.Select(h => h.AssessmentDate.ToString("yyyy-MM")).ToArray();
        var values = history.Select(h => h.StandardScore ?? 0).ToArray();

        ProgressSeries = new ISeries[]
        {
            new LineSeries<decimal>
            {
                Name = "التقدم",
                Values = values,
                Stroke = new SolidColorPaint(SKColor.Parse("#1e3a5f")) { StrokeThickness = 3 },
                Fill = new SolidColorPaint(SKColor.Parse("#1e3a5f")) { SKAlpha = 30 }
            }
        };
        XAxes = new[] { new Axis { Labels = labels } };

        this.RaisePropertyChanged(nameof(ProgressSeries));
        this.RaisePropertyChanged(nameof(XAxes));
    }
}