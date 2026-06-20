using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class TelehealthViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    private ObservableCollection<TelehealthSession> _sessions = new();
    public ObservableCollection<TelehealthSession> Sessions
    {
        get => _sessions;
        set => this.RaiseAndSetIfChanged(ref _sessions, value);
    }

    private TelehealthSession? _selectedSession;
    public TelehealthSession? SelectedSession
    {
        get => _selectedSession;
        set => this.RaiseAndSetIfChanged(ref _selectedSession, value);
    }

    private bool _isFormOpen;
    public bool IsFormOpen
    {
        get => _isFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isFormOpen, value);
    }

    private Session? _selectedSessionBase;
    public Session? SelectedSessionBase
    {
        get => _selectedSessionBase;
        set => this.RaiseAndSetIfChanged(ref _selectedSessionBase, value);
    }

    private string _platform = "Zoom";
    public string Platform
    {
        get => _platform;
        set => this.RaiseAndSetIfChanged(ref _platform, value);
    }

    private string? _meetingLink;
    public string? MeetingLink
    {
        get => _meetingLink;
        set => this.RaiseAndSetIfChanged(ref _meetingLink, value);
    }

    public ObservableCollection<Session> AvailableSessions { get; } = new();

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> StartSessionCommand { get; }
    public ReactiveCommand<Unit, Unit> SendLinkCommand { get; }

    public TelehealthViewModel(DatabaseService dbService, DialogService dialogService)
    {
        _dbService = dbService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var sessions = await _dbService.GetTodaySessionsAsync();
            AvailableSessions.Clear();
            foreach (var s in sessions) AvailableSessions.Add(s);
            IsFormOpen = true;
        });
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(() => { IsFormOpen = false; });
        StartSessionCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedSession != null && !string.IsNullOrEmpty(SelectedSession.MeetingLink))
            {
                // Open link in browser
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = SelectedSession.MeetingLink,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
        });
        SendLinkCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedSession != null)
            {
                await _dialogService.ShowInfoAsync("إرسال", "تم إرسال رابط الجلسة إلى ولي الأمر");
            }
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _dbService.GetTelehealthSessionsAsync();
        Sessions.Clear();
        foreach (var s in list) Sessions.Add(s);
    }

    private async Task SaveAsync()
    {
        if (SelectedSessionBase == null) return;

        var telehealth = new TelehealthSession
        {
            SessionId = SelectedSessionBase.Id,
            Platform = Platform,
            MeetingLink = MeetingLink,
            Status = "Scheduled"
        };

        await _dbService.AddTelehealthSessionAsync(telehealth);
        IsFormOpen = false;
        await LoadAsync();
        await AuditLogger.LogAsync("Create", "Telehealth", $"Created telehealth session for {SelectedSessionBase.Beneficiary.Name}");
    }
}