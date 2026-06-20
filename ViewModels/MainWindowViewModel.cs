using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;

namespace RehabCenterApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly NavigationService _navigationService;
    private readonly NotificationService _notificationService;
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    public ViewModelBase CurrentViewModel => _navigationService.CurrentViewModel;

    private bool _isPaneOpen = true;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchQuery, value);
            _ = SearchAsync();
        }
    }

    public ObservableCollection<Reminder> UpcomingReminders { get; } = new();

    // Navigation Commands
    public ReactiveCommand<Unit, Unit> NavigateToDashboardCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToBeneficiariesCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToSessionsCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToAccountingCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToCorrespondenceCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToRemindersCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToFormsCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToSettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToAssessmentsCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToInterventionPlansCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToWaitingListCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToInventoryCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToParentPortalCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToMDTMeetingsCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToAnalyticsCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToTelehealthCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToClinicalReportsCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToGamificationCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToGovernmentReportsCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToDocumentArchiveCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToHRManagementCommand { get; }
    public ReactiveCommand<Unit, Unit> TogglePaneCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleLanguageCommand { get; }
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

    public MainWindowViewModel(
        NavigationService navigationService,
        NotificationService notificationService,
        DatabaseService dbService,
        DialogService dialogService)
    {
        _navigationService = navigationService;
        _notificationService = notificationService;
        _dbService = dbService;
        _dialogService = dialogService;

        _navigationService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_navigationService.CurrentViewModel))
                this.RaisePropertyChanged(nameof(CurrentViewModel));
        };

        NavigateToDashboardCommand = ReactiveCommand.Create(() => _navigationService.NavigateToDashboard());
        NavigateToBeneficiariesCommand = ReactiveCommand.Create(() => _navigationService.NavigateToBeneficiaries());
        NavigateToSessionsCommand = ReactiveCommand.Create(() => _navigationService.NavigateToSessions());
        NavigateToAccountingCommand = ReactiveCommand.Create(() => _navigationService.NavigateToAccounting());
        NavigateToCorrespondenceCommand = ReactiveCommand.Create(() => _navigationService.NavigateToCorrespondence());
        NavigateToRemindersCommand = ReactiveCommand.Create(() => _navigationService.NavigateToReminders());
        NavigateToFormsCommand = ReactiveCommand.Create(() => _navigationService.NavigateToForms());
        NavigateToSettingsCommand = ReactiveCommand.Create(() => _navigationService.NavigateToSettings());
        NavigateToAssessmentsCommand = ReactiveCommand.Create(() => _navigationService.NavigateToAssessments());
        NavigateToInterventionPlansCommand = ReactiveCommand.Create(() => _navigationService.NavigateToInterventionPlans());
        NavigateToWaitingListCommand = ReactiveCommand.Create(() => _navigationService.NavigateToWaitingList());
        NavigateToInventoryCommand = ReactiveCommand.Create(() => _navigationService.NavigateToInventory());
        NavigateToParentPortalCommand = ReactiveCommand.Create(() => _navigationService.NavigateToParentPortal());
        NavigateToMDTMeetingsCommand = ReactiveCommand.Create(() => _navigationService.NavigateToMDTMeetings());
        NavigateToAnalyticsCommand = ReactiveCommand.Create(() => _navigationService.NavigateToAnalytics());
        NavigateToTelehealthCommand = ReactiveCommand.Create(() => _navigationService.NavigateToTelehealth());
        NavigateToClinicalReportsCommand = ReactiveCommand.Create(() => _navigationService.NavigateToClinicalReports());
        NavigateToGamificationCommand = ReactiveCommand.Create(() => _navigationService.NavigateToGamification());
        NavigateToGovernmentReportsCommand = ReactiveCommand.Create(() => _navigationService.NavigateToGovernmentReports());
        NavigateToDocumentArchiveCommand = ReactiveCommand.Create(() => _navigationService.NavigateToDocumentArchive());
        NavigateToHRManagementCommand = ReactiveCommand.Create(() => _navigationService.NavigateToHRManagement());
        TogglePaneCommand = ReactiveCommand.Create(() => { IsPaneOpen = !IsPaneOpen; });
        ToggleLanguageCommand = ReactiveCommand.Create(() => Services.LocalizationService.Instance.ToggleLanguage());
        LogoutCommand = ReactiveCommand.CreateFromTask(LogoutAsync);

        _notificationService.ReminderTriggered += (s, r) =>
        {
            // Handle notification in UI thread
        };

        _notificationService.Start();
        _ = LoadRemindersAsync();
    }

    private async Task LoadRemindersAsync()
    {
        var reminders = await _dbService.GetUpcomingRemindersAsync(7);
        UpcomingReminders.Clear();
        foreach (var r in reminders)
            UpcomingReminders.Add(r);
    }

    private async Task SearchAsync()
    {
        // Global search implementation
    }

    private async Task LogoutAsync()
    {
        var confirm = await _dialogService.ShowConfirmAsync("تأكيد", "هل تريد تسجيل الخروج؟");
        if (confirm)
        {
            Environment.Exit(0);
        }
    }
}