using System;
using ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Services;

public class NavigationService : ReactiveObject
{
    private ViewModelBase _currentViewModel = null!;
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    private readonly DashboardViewModel _dashboardVm;
    private readonly BeneficiariesViewModel _beneficiariesVm;
    private readonly SessionsViewModel _sessionsVm;
    private readonly AccountingViewModel _accountingVm;
    private readonly CorrespondenceViewModel _correspondenceVm;
    private readonly RemindersViewModel _remindersVm;
    private readonly FormsViewModel _formsVm;
    private readonly SettingsViewModel _settingsVm;
    private readonly AssessmentsViewModel _assessmentsVm;
    private readonly InterventionPlansViewModel _interventionVm;
    private readonly WaitingListViewModel _waitingVm;
    private readonly InventoryViewModel _inventoryVm;
    private readonly ParentPortalViewModel _parentVm;
    private readonly MDTMeetingsViewModel _mdtVm;
    private readonly AnalyticsViewModel _analyticsVm;
    private readonly TelehealthViewModel _telehealthVm;
    private readonly ClinicalReportsViewModel _clinicalVm;
    private readonly GamificationViewModel _gamificationVm;
    private readonly GovernmentReportsViewModel _govVm;
    private readonly DocumentArchiveViewModel _docVm;
    private readonly HRManagementViewModel _hrVm;
    private readonly UserManagementViewModel _userMgmtVm;

    public NavigationService(
        DashboardViewModel dashboardVm,
        BeneficiariesViewModel beneficiariesVm,
        SessionsViewModel sessionsVm,
        AccountingViewModel accountingVm,
        CorrespondenceViewModel correspondenceVm,
        RemindersViewModel remindersVm,
        FormsViewModel formsVm,
        SettingsViewModel settingsVm,
        AssessmentsViewModel assessmentsVm,
        InterventionPlansViewModel interventionVm,
        WaitingListViewModel waitingVm,
        InventoryViewModel inventoryVm,
        ParentPortalViewModel parentVm,
        MDTMeetingsViewModel mdtVm,
        AnalyticsViewModel analyticsVm,
        TelehealthViewModel telehealthVm,
        ClinicalReportsViewModel clinicalVm,
        GamificationViewModel gamificationVm,
        GovernmentReportsViewModel govVm,
        DocumentArchiveViewModel docVm,
        HRManagementViewModel hrVm,
        UserManagementViewModel userMgmtVm)
    {
        _dashboardVm = dashboardVm;
        _beneficiariesVm = beneficiariesVm;
        _sessionsVm = sessionsVm;
        _accountingVm = accountingVm;
        _correspondenceVm = correspondenceVm;
        _remindersVm = remindersVm;
        _formsVm = formsVm;
        _settingsVm = settingsVm;
        _assessmentsVm = assessmentsVm;
        _interventionVm = interventionVm;
        _waitingVm = waitingVm;
        _inventoryVm = inventoryVm;
        _parentVm = parentVm;
        _mdtVm = mdtVm;
        _analyticsVm = analyticsVm;
        _telehealthVm = telehealthVm;
        _clinicalVm = clinicalVm;
        _gamificationVm = gamificationVm;
        _govVm = govVm;
        _docVm = docVm;
        _hrVm = hrVm;
        _userMgmtVm = userMgmtVm;

        CurrentViewModel = _dashboardVm;
    }

    public void NavigateToDashboard() => CurrentViewModel = _dashboardVm;
    public void NavigateToBeneficiaries() => CurrentViewModel = _beneficiariesVm;
    public void NavigateToSessions() => CurrentViewModel = _sessionsVm;
    public void NavigateToAccounting() => CurrentViewModel = _accountingVm;
    public void NavigateToCorrespondence() => CurrentViewModel = _correspondenceVm;
    public void NavigateToReminders() => CurrentViewModel = _remindersVm;
    public void NavigateToForms() => CurrentViewModel = _formsVm;
    public void NavigateToSettings() => CurrentViewModel = _settingsVm;
    public void NavigateToAssessments() => CurrentViewModel = _assessmentsVm;
    public void NavigateToInterventionPlans() => CurrentViewModel = _interventionVm;
    public void NavigateToWaitingList() => CurrentViewModel = _waitingVm;
    public void NavigateToInventory() => CurrentViewModel = _inventoryVm;
    public void NavigateToParentPortal() => CurrentViewModel = _parentVm;
    public void NavigateToMDTMeetings() => CurrentViewModel = _mdtVm;
    public void NavigateToAnalytics() => CurrentViewModel = _analyticsVm;
    public void NavigateToTelehealth() => CurrentViewModel = _telehealthVm;
    public void NavigateToClinicalReports() => CurrentViewModel = _clinicalVm;
    public void NavigateToGamification() => CurrentViewModel = _gamificationVm;
    public void NavigateToGovernmentReports() => CurrentViewModel = _govVm;
    public void NavigateToDocumentArchive() => CurrentViewModel = _docVm;
    public void NavigateToHRManagement() => CurrentViewModel = _hrVm;
    public void NavigateToUserManagement() => CurrentViewModel = _userMgmtVm;
}
