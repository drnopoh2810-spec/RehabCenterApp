using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;

namespace RehabCenterApp.ViewModels;

public class RemindersViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;

    private ObservableCollection<Reminder> _reminders = new();
    public ObservableCollection<Reminder> Reminders
    {
        get => _reminders;
        set => this.RaiseAndSetIfChanged(ref _reminders, value);
    }

    private ObservableCollection<Reminder> _overdueReminders = new();
    public ObservableCollection<Reminder> OverdueReminders => _overdueReminders;

    private ObservableCollection<Reminder> _todayReminders = new();
    public ObservableCollection<Reminder> TodayReminders => _todayReminders;

    private ObservableCollection<Reminder> _upcomingReminders = new();
    public ObservableCollection<Reminder> UpcomingReminders => _upcomingReminders;

    private Reminder? _selectedReminder;
    public Reminder? SelectedReminder
    {
        get => _selectedReminder;
        set => this.RaiseAndSetIfChanged(ref _selectedReminder, value);
    }

    private bool _isFormOpen;
    public bool IsFormOpen
    {
        get => _isFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isFormOpen, value);
    }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private string? _description;
    public string? Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    private DateTime _reminderDate = DateTime.Now;
    public DateTime ReminderDate
    {
        get => _reminderDate;
        set => this.RaiseAndSetIfChanged(ref _reminderDate, value);
    }

    private TimeSpan _reminderTime = new TimeSpan(9, 0, 0);
    public TimeSpan ReminderTime
    {
        get => _reminderTime;
        set => this.RaiseAndSetIfChanged(ref _reminderTime, value);
    }

    private string _type = "General";
    public string Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    private string _priority = "Medium";
    public string Priority
    {
        get => _priority;
        set => this.RaiseAndSetIfChanged(ref _priority, value);
    }

    private Beneficiary? _linkedBeneficiary;
    public Beneficiary? LinkedBeneficiary
    {
        get => _linkedBeneficiary;
        set => this.RaiseAndSetIfChanged(ref _linkedBeneficiary, value);
    }

    public ObservableCollection<Beneficiary> Beneficiaries { get; } = new();

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> CompleteCommand { get; }
    public ReactiveCommand<Unit, Unit> SnoozeCommand { get; }

    public RemindersViewModel(DatabaseService dbService)
    {
        _dbService = dbService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var list = await _dbService.GetBeneficiariesAsync();
            Beneficiaries.Clear();
            foreach (var b in list) Beneficiaries.Add(b);
            IsFormOpen = true;
        });
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(() => { IsFormOpen = false; });
        CompleteCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedReminder != null)
            {
                SelectedReminder.Status = "Completed";
                await _dbService.UpdateReminderAsync(SelectedReminder);
                await LoadAsync();
            }
        });
        SnoozeCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedReminder != null)
            {
                SelectedReminder.DateTime = SelectedReminder.DateTime.AddHours(1);
                SelectedReminder.Status = "Pending";
                await _dbService.UpdateReminderAsync(SelectedReminder);
                await LoadAsync();
            }
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var all = await _dbService.GetRemindersAsync();
        Reminders.Clear();
        foreach (var r in all) Reminders.Add(r);

        var now = DateTime.Now;
        _overdueReminders.Clear();
        _todayReminders.Clear();
        _upcomingReminders.Clear();

        foreach (var r in all.Where(r => r.Status == "Pending"))
        {
            if (r.DateTime < now)
                _overdueReminders.Add(r);
            else if (r.DateTime.Date == now.Date)
                _todayReminders.Add(r);
            else
                _upcomingReminders.Add(r);
        }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
            return;

        var reminder = new Reminder
        {
            Title = Title,
            Description = Description,
            DateTime = ReminderDate.Date + ReminderTime,
            Type = Type,
            Priority = Priority,
            BeneficiaryId = LinkedBeneficiary?.Id,
            Status = "Pending"
        };

        await _dbService.AddReminderAsync(reminder);
        IsFormOpen = false;
        Title = string.Empty;
        Description = null;
        await LoadAsync();
    }
}