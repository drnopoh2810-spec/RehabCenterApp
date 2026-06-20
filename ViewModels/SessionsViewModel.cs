using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;

namespace RehabCenterApp.ViewModels;

public class SessionsViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;

    private ObservableCollection<Session> _sessions = new();
    public ObservableCollection<Session> Sessions
    {
        get => _sessions;
        set => this.RaiseAndSetIfChanged(ref _sessions, value);
    }

    private DateTime _selectedDate = DateTime.Now;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedDate, value);
            _ = LoadSessionsAsync();
        }
    }

    private Session? _selectedSession;
    public Session? SelectedSession
    {
        get => _selectedSession;
        set => this.RaiseAndSetIfChanged(ref _selectedSession, value);
    }

    private ObservableCollection<Beneficiary> _beneficiaries = new();
    public ObservableCollection<Beneficiary> Beneficiaries
    {
        get => _beneficiaries;
        set => this.RaiseAndSetIfChanged(ref _beneficiaries, value);
    }

    private ObservableCollection<Employee> _therapists = new();
    public ObservableCollection<Employee> Therapists
    {
        get => _therapists;
        set => this.RaiseAndSetIfChanged(ref _therapists, value);
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

    private Employee? _selectedTherapist;
    public Employee? SelectedTherapist
    {
        get => _selectedTherapist;
        set => this.RaiseAndSetIfChanged(ref _selectedTherapist, value);
    }

    private string _sessionType = string.Empty;
    public string SessionType
    {
        get => _sessionType;
        set => this.RaiseAndSetIfChanged(ref _sessionType, value);
    }

    private TimeSpan _sessionTime = new TimeSpan(9, 0, 0);
    public TimeSpan SessionTime
    {
        get => _sessionTime;
        set => this.RaiseAndSetIfChanged(ref _sessionTime, value);
    }

    private int _duration = 45;
    public int Duration
    {
        get => _duration;
        set => this.RaiseAndSetIfChanged(ref _duration, value);
    }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> MarkPresentCommand { get; }
    public ReactiveCommand<Unit, Unit> MarkAbsentCommand { get; }
    public ReactiveCommand<Unit, Unit> MarkCancelledCommand { get; }

    public SessionsViewModel(DatabaseService dbService)
    {
        _dbService = dbService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadSessionsAsync);
        AddCommand = ReactiveCommand.Create(() =>
        {
            IsFormOpen = true;
            _ = LoadDropdownsAsync();
        });
        SaveCommand = ReactiveCommand.CreateFromTask(SaveSessionAsync);
        CancelCommand = ReactiveCommand.Create(() => { IsFormOpen = false; });
        MarkPresentCommand = ReactiveCommand.CreateFromTask(async () => await UpdateStatusAsync("Present"));
        MarkAbsentCommand = ReactiveCommand.CreateFromTask(async () => await UpdateStatusAsync("Absent"));
        MarkCancelledCommand = ReactiveCommand.CreateFromTask(async () => await UpdateStatusAsync("Cancelled"));

        _ = LoadSessionsAsync();
    }

    private async Task LoadSessionsAsync()
    {
        var list = await _dbService.GetSessionsAsync(SelectedDate);
        Sessions.Clear();
        foreach (var s in list)
            Sessions.Add(s);
    }

    private async Task LoadDropdownsAsync()
    {
        var beneficiaries = await _dbService.GetBeneficiariesAsync();
        Beneficiaries.Clear();
        foreach (var b in beneficiaries)
            Beneficiaries.Add(b);

        var therapists = await _dbService.GetEmployeesAsync("Therapist");
        Therapists.Clear();
        foreach (var t in therapists)
            Therapists.Add(t);
    }

    private async Task SaveSessionAsync()
    {
        if (SelectedBeneficiary == null || SelectedTherapist == null)
            return;

        var session = new Session
        {
            BeneficiaryId = SelectedBeneficiary.Id,
            TherapistId = SelectedTherapist.Id,
            SessionType = SessionType,
            Date = SelectedDate,
            Time = SessionTime,
            Duration = Duration,
            Notes = Notes,
            Status = "Scheduled"
        };

        await _dbService.AddSessionAsync(session);
        IsFormOpen = false;
        await LoadSessionsAsync();
    }

    private async Task UpdateStatusAsync(string status)
    {
        if (SelectedSession != null)
        {
            SelectedSession.Status = status;
            await _dbService.UpdateSessionAsync(SelectedSession);
            await LoadSessionsAsync();
        }
    }
}