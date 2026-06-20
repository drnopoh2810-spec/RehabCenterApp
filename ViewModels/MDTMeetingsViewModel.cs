using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class MDTMeetingsViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    private ObservableCollection<MDTMeeting> _meetings = new();
    public ObservableCollection<MDTMeeting> Meetings
    {
        get => _meetings;
        set => this.RaiseAndSetIfChanged(ref _meetings, value);
    }

    private MDTMeeting? _selectedMeeting;
    public MDTMeeting? SelectedMeeting
    {
        get => _selectedMeeting;
        set => this.RaiseAndSetIfChanged(ref _selectedMeeting, value);
    }

    private bool _isFormOpen;
    public bool IsFormOpen
    {
        get => _isFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isFormOpen, value);
    }

    private Beneficiary? _selectedBeneficiary;
    public Beneficiary? SelectedBeneficiary
    {
        get => _selectedBeneficiary;
        set => this.RaiseAndSetIfChanged(ref _selectedBeneficiary, value);
    }

    private DateTime _meetingDate = DateTime.Now;
    public DateTime MeetingDate
    {
        get => _meetingDate;
        set => this.RaiseAndSetIfChanged(ref _meetingDate, value);
    }

    private string _discussionPoints = string.Empty;
    public string DiscussionPoints
    {
        get => _discussionPoints;
        set => this.RaiseAndSetIfChanged(ref _discussionPoints, value);
    }

    private string _decisions = string.Empty;
    public string Decisions
    {
        get => _decisions;
        set => this.RaiseAndSetIfChanged(ref _decisions, value);
    }

    public ObservableCollection<Beneficiary> Beneficiaries { get; } = new();
    public ObservableCollection<Employee> Therapists { get; } = new();

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> CompleteCommand { get; }
    public ReactiveCommand<Unit, Unit> PrintMinutesCommand { get; }

    public MDTMeetingsViewModel(DatabaseService dbService, DialogService dialogService)
    {
        _dbService = dbService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var bList = await _dbService.GetBeneficiariesAsync();
            Beneficiaries.Clear();
            foreach (var b in bList) Beneficiaries.Add(b);

            var tList = await _dbService.GetEmployeesAsync();
            Therapists.Clear();
            foreach (var t in tList) Therapists.Add(t);

            IsFormOpen = true;
        });
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(() => { IsFormOpen = false; });
        CompleteCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedMeeting != null)
            {
                SelectedMeeting.Status = "Completed";
                await _dbService.UpdateMDTMeetingAsync(SelectedMeeting);
                await LoadAsync();
            }
        });
        PrintMinutesCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedMeeting != null)
                await _dialogService.ShowInfoAsync("طباعة", "تم طباعة محضر الاجتماع");
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _dbService.GetMDTMeetingsAsync();
        Meetings.Clear();
        foreach (var m in list) Meetings.Add(m);
    }

    private async Task SaveAsync()
    {
        if (SelectedBeneficiary == null) return;

        var meeting = new MDTMeeting
        {
            BeneficiaryId = SelectedBeneficiary.Id,
            MeetingDate = MeetingDate,
            DiscussionPoints = DiscussionPoints,
            Decisions = Decisions,
            Status = "Scheduled"
        };

        await _dbService.AddMDTMeetingAsync(meeting);
        IsFormOpen = false;
        DiscussionPoints = string.Empty;
        Decisions = string.Empty;
        await LoadAsync();
        await AuditLogger.LogAsync("Create", "MDTMeeting", $"Scheduled MDT for {SelectedBeneficiary.Name}");
    }
}