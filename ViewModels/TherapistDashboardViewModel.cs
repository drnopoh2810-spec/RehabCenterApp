using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class TherapistDashboardViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly PrintService _printService;
    private readonly WordExportService _wordExportService;
    private readonly DialogService _dialogService;
    private readonly Action _onLogout;

    public string TherapistName { get; private set; } = string.Empty;
    public int TherapistEmployeeId { get; private set; }

    private ObservableCollection<Session> _todaySessions = new();
    public ObservableCollection<Session> TodaySessions
    {
        get => _todaySessions;
        set => this.RaiseAndSetIfChanged(ref _todaySessions, value);
    }

    private Session? _selectedSession;
    public Session? SelectedSession
    {
        get => _selectedSession;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSession, value);
            if (value != null)
                _ = LoadSessionDataAsync(value);
        }
    }

    private TherapistReport? _currentReport;
    public TherapistReport? CurrentReport
    {
        get => _currentReport;
        set => this.RaiseAndSetIfChanged(ref _currentReport, value);
    }

    private ObservableCollection<Session> _historyList = new();
    public ObservableCollection<Session> HistoryList
    {
        get => _historyList;
        set => this.RaiseAndSetIfChanged(ref _historyList, value);
    }

    private ObservableCollection<TherapistReport> _historyReports = new();
    public ObservableCollection<TherapistReport> HistoryReports
    {
        get => _historyReports;
        set => this.RaiseAndSetIfChanged(ref _historyReports, value);
    }

    private ObservableCollection<TherapistReport> _allReports = new();
    public ObservableCollection<TherapistReport> AllReports
    {
        get => _allReports;
        set => this.RaiseAndSetIfChanged(ref _allReports, value);
    }

    private bool _isReportFormOpen;
    public bool IsReportFormOpen
    {
        get => _isReportFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isReportFormOpen, value);
    }

    private bool _isHistoryOpen;
    public bool IsHistoryOpen
    {
        get => _isHistoryOpen;
        set => this.RaiseAndSetIfChanged(ref _isHistoryOpen, value);
    }

    private int _selectedTab;
    public int SelectedTab
    {
        get => _selectedTab;
        set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
    }

    private Beneficiary? _historyBeneficiary;
    public Beneficiary? HistoryBeneficiary
    {
        get => _historyBeneficiary;
        set => this.RaiseAndSetIfChanged(ref _historyBeneficiary, value);
    }

    // Report form fields
    private string _activities = string.Empty;
    public string Activities
    {
        get => _activities;
        set => this.RaiseAndSetIfChanged(ref _activities, value);
    }

    private string _childResponse = string.Empty;
    public string ChildResponse
    {
        get => _childResponse;
        set => this.RaiseAndSetIfChanged(ref _childResponse, value);
    }

    private string _goalsAddressed = string.Empty;
    public string GoalsAddressed
    {
        get => _goalsAddressed;
        set => this.RaiseAndSetIfChanged(ref _goalsAddressed, value);
    }

    private string _homework = string.Empty;
    public string Homework
    {
        get => _homework;
        set => this.RaiseAndSetIfChanged(ref _homework, value);
    }

    private string _behaviorNotes = string.Empty;
    public string BehaviorNotes
    {
        get => _behaviorNotes;
        set => this.RaiseAndSetIfChanged(ref _behaviorNotes, value);
    }

    private string _nextSessionPlan = string.Empty;
    public string NextSessionPlan
    {
        get => _nextSessionPlan;
        set => this.RaiseAndSetIfChanged(ref _nextSessionPlan, value);
    }

    private int _overallRating = 3;
    public int OverallRating
    {
        get => _overallRating;
        set => this.RaiseAndSetIfChanged(ref _overallRating, value);
    }

    private int _totalSessions;
    public int TotalSessions
    {
        get => _totalSessions;
        set => this.RaiseAndSetIfChanged(ref _totalSessions, value);
    }

    private int _totalReports;
    public int TotalReports
    {
        get => _totalReports;
        set => this.RaiseAndSetIfChanged(ref _totalReports, value);
    }

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenReportCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveReportCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelReportCommand { get; }
    public ReactiveCommand<Unit, Unit> ViewChildHistoryCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseHistoryCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportHistoryPdfCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportHistoryWordCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleLanguageCommand { get; }
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

    public TherapistDashboardViewModel(
        DatabaseService dbService,
        PrintService printService,
        WordExportService wordExportService,
        DialogService dialogService,
        string therapistUsername,
        Action onLogout)
    {
        _dbService = dbService;
        _printService = printService;
        _wordExportService = wordExportService;
        _dialogService = dialogService;
        _onLogout = onLogout;
        TherapistName = therapistUsername;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        OpenReportCommand = ReactiveCommand.Create(OpenReportForm);
        SaveReportCommand = ReactiveCommand.CreateFromTask(SaveReportAsync);
        CancelReportCommand = ReactiveCommand.Create(() => { IsReportFormOpen = false; });
        ViewChildHistoryCommand = ReactiveCommand.CreateFromTask(LoadChildHistoryAsync);
        CloseHistoryCommand = ReactiveCommand.Create(() => { IsHistoryOpen = false; });
        ExportHistoryPdfCommand = ReactiveCommand.CreateFromTask(ExportHistoryPdfAsync);
        ExportHistoryWordCommand = ReactiveCommand.CreateFromTask(ExportHistoryWordAsync);
        ToggleLanguageCommand = ReactiveCommand.Create(() => LocalizationService.Instance.ToggleLanguage());
        LogoutCommand = ReactiveCommand.CreateFromTask(LogoutAsync);

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var employee = await _dbService.GetEmployeeByUsernameAsync(TherapistName);
        if (employee != null)
        {
            TherapistEmployeeId = employee.Id;
            this.RaisePropertyChanged(nameof(TherapistName));
        }

        await LoadTodaySessionsAsync();
        await LoadAllReportsAsync();
        TotalSessions = TodaySessions.Count;
        TotalReports = AllReports.Count;
    }

    private async Task LoadTodaySessionsAsync()
    {
        List<Session> sessions;
        if (TherapistEmployeeId > 0)
            sessions = await _dbService.GetTherapistSessionsAsync(TherapistEmployeeId, DateTime.Now);
        else
            sessions = await _dbService.GetSessionsAsync(DateTime.Now);

        TodaySessions.Clear();
        foreach (var s in sessions)
            TodaySessions.Add(s);

        TotalSessions = TodaySessions.Count;
    }

    private async Task LoadAllReportsAsync()
    {
        var reports = TherapistEmployeeId > 0
            ? await _dbService.GetTherapistReportsAsync(TherapistEmployeeId)
            : await _dbService.GetTherapistReportsAsync();

        AllReports.Clear();
        foreach (var r in reports)
            AllReports.Add(r);

        TotalReports = AllReports.Count;
    }

    private async Task LoadSessionDataAsync(Session session)
    {
        var report = await _dbService.GetTherapistReportBySessionAsync(session.Id);
        CurrentReport = report;

        if (report != null)
        {
            Activities = report.ActivitiesPerformed;
            ChildResponse = report.ChildResponse;
            GoalsAddressed = report.GoalsAddressed;
            Homework = report.Homework;
            BehaviorNotes = report.BehaviorNotes;
            NextSessionPlan = report.NextSessionPlan;
            OverallRating = report.OverallRating;
        }
        else
        {
            ClearReportForm();
        }
    }

    private void OpenReportForm()
    {
        if (SelectedSession == null) return;

        if (CurrentReport != null)
        {
            Activities = CurrentReport.ActivitiesPerformed;
            ChildResponse = CurrentReport.ChildResponse;
            GoalsAddressed = CurrentReport.GoalsAddressed;
            Homework = CurrentReport.Homework;
            BehaviorNotes = CurrentReport.BehaviorNotes;
            NextSessionPlan = CurrentReport.NextSessionPlan;
            OverallRating = CurrentReport.OverallRating;
        }
        else
        {
            ClearReportForm();
        }

        IsReportFormOpen = true;
    }

    private void ClearReportForm()
    {
        Activities = string.Empty;
        ChildResponse = string.Empty;
        GoalsAddressed = string.Empty;
        Homework = string.Empty;
        BehaviorNotes = string.Empty;
        NextSessionPlan = string.Empty;
        OverallRating = 3;
    }

    private async Task SaveReportAsync()
    {
        if (SelectedSession == null) return;

        if (CurrentReport == null)
        {
            var report = new TherapistReport
            {
                SessionId = SelectedSession.Id,
                TherapistId = TherapistEmployeeId > 0 ? TherapistEmployeeId : SelectedSession.TherapistId,
                ReportDate = DateTime.Now,
                ActivitiesPerformed = Activities,
                ChildResponse = ChildResponse,
                GoalsAddressed = GoalsAddressed,
                Homework = Homework,
                BehaviorNotes = BehaviorNotes,
                NextSessionPlan = NextSessionPlan,
                OverallRating = OverallRating,
                Status = "Final"
            };
            CurrentReport = await _dbService.AddTherapistReportAsync(report);
        }
        else
        {
            CurrentReport.ActivitiesPerformed = Activities;
            CurrentReport.ChildResponse = ChildResponse;
            CurrentReport.GoalsAddressed = GoalsAddressed;
            CurrentReport.Homework = Homework;
            CurrentReport.BehaviorNotes = BehaviorNotes;
            CurrentReport.NextSessionPlan = NextSessionPlan;
            CurrentReport.OverallRating = OverallRating;
            CurrentReport.Status = "Final";
            await _dbService.UpdateTherapistReportAsync(CurrentReport);
        }

        IsReportFormOpen = false;
        await LoadAllReportsAsync();
        await _dialogService.ShowInfoAsync("✓", "Report saved successfully.");
    }

    private async Task LoadChildHistoryAsync()
    {
        if (SelectedSession?.Beneficiary == null) return;

        HistoryBeneficiary = SelectedSession.Beneficiary;
        var sessions = await _dbService.GetChildHistorySessionsAsync(SelectedSession.BeneficiaryId);
        var reports = await _dbService.GetTherapistReportsAsync(beneficiaryId: SelectedSession.BeneficiaryId);

        HistoryList.Clear();
        foreach (var s in sessions) HistoryList.Add(s);

        HistoryReports.Clear();
        foreach (var r in reports) HistoryReports.Add(r);

        IsHistoryOpen = true;
    }

    private async Task ExportHistoryPdfAsync()
    {
        if (HistoryBeneficiary == null) return;

        var sessions = HistoryList.ToList();
        var reports = HistoryReports.ToList();
        var filePath = await _printService.ExportChildHistoryToPdfAsync(HistoryBeneficiary, sessions, reports);
        await _dialogService.ShowInfoAsync("Exported", $"PDF saved to:\n{filePath}");
    }

    private async Task ExportHistoryWordAsync()
    {
        if (HistoryBeneficiary == null) return;

        var sessions = HistoryList.ToList();
        var reports = HistoryReports.ToList();
        var filePath = await _wordExportService.ExportChildHistoryAsync(HistoryBeneficiary, sessions, reports);
        await _dialogService.ShowInfoAsync("Exported", $"Word document saved to:\n{filePath}");
    }

    private async Task LogoutAsync()
    {
        var confirm = await _dialogService.ShowConfirmAsync("Logout", "Are you sure you want to logout?");
        if (confirm)
            _onLogout();
    }
}
