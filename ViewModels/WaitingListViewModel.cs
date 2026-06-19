using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class WaitingListViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    private ObservableCollection<WaitingListEntry> _entries = new();
    public ObservableCollection<WaitingListEntry> Entries
    {
        get => _entries;
        set => this.RaiseAndSetIfChanged(ref _entries, value);
    }

    private WaitingListEntry? _selectedEntry;
    public WaitingListEntry? SelectedEntry
    {
        get => _selectedEntry;
        set => this.RaiseAndSetIfChanged(ref _selectedEntry, value);
    }

    private bool _isFormOpen;
    public bool IsFormOpen
    {
        get => _isFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isFormOpen, value);
    }

    private string _entryName = string.Empty;
    public string EntryName
    {
        get => _entryName;
        set => this.RaiseAndSetIfChanged(ref _entryName, value);
    }

    private DateTime _dateOfBirth = DateTime.Now.AddYears(-5);
    public DateTime DateOfBirth
    {
        get => _dateOfBirth;
        set => this.RaiseAndSetIfChanged(ref _dateOfBirth, value);
    }

    private string _disabilityType = string.Empty;
    public string DisabilityType
    {
        get => _disabilityType;
        set => this.RaiseAndSetIfChanged(ref _disabilityType, value);
    }

    private string? _guardianPhone;
    public string? GuardianPhone
    {
        get => _guardianPhone;
        set => this.RaiseAndSetIfChanged(ref _guardianPhone, value);
    }

    private int _priority = 3;
    public int Priority
    {
        get => _priority;
        set => this.RaiseAndSetIfChanged(ref _priority, value);
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
    public ReactiveCommand<Unit, Unit> ConvertToBeneficiaryCommand { get; }
    public ReactiveCommand<Unit, Unit> ContactCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    public WaitingListViewModel(DatabaseService dbService, DialogService dialogService)
    {
        _dbService = dbService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddCommand = ReactiveCommand.Create(() => IsFormOpen = true);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(() => IsFormOpen = false);
        ConvertToBeneficiaryCommand = ReactiveCommand.CreateFromTask(ConvertAsync);
        ContactCommand = ReactiveCommand.CreateFromTask(ContactAsync);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync);

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _dbService.GetWaitingListAsync();
        Entries.Clear();
        foreach (var e in list) Entries.Add(e);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EntryName)) return;

        var entry = new WaitingListEntry
        {
            Name = EntryName,
            DateOfBirth = DateOfBirth,
            DisabilityType = DisabilityType,
            GuardianPhone = GuardianPhone,
            Priority = Priority,
            Notes = Notes,
            Status = "Waiting"
        };

        await _dbService.AddWaitingListEntryAsync(entry);
        IsFormOpen = false;
        EntryName = string.Empty;
        await LoadAsync();
    }

    private async Task ConvertAsync()
    {
        if (SelectedEntry == null) return;
        var confirm = await _dialogService.ShowConfirmAsync("تحويل", $"تحويل {SelectedEntry.Name} إلى مستفيد؟");
        if (!confirm) return;

        var beneficiary = new Beneficiary
        {
            Name = SelectedEntry.Name,
            DateOfBirth = SelectedEntry.DateOfBirth,
            Gender = SelectedEntry.Gender,
            DisabilityType = SelectedEntry.DisabilityType,
            GuardianPhone = SelectedEntry.GuardianPhone,
            Status = "Active"
        };

        await _dbService.AddBeneficiaryAsync(beneficiary);
        SelectedEntry.Status = "Converted";
        SelectedEntry.ConvertedToBeneficiaryId = beneficiary.Id;
        await _dbService.UpdateWaitingListEntryAsync(SelectedEntry);
        await LoadAsync();
        await AuditLogger.LogAsync("Convert", "WaitingList", $"Converted {SelectedEntry.Name} to beneficiary");
    }

    private async Task ContactAsync()
    {
        if (SelectedEntry == null) return;
        SelectedEntry.ContactDate = DateTime.Now;
        SelectedEntry.Status = "Contacted";
        await _dbService.UpdateWaitingListEntryAsync(SelectedEntry);
        await _dialogService.ShowInfoAsync("تم", "تم تسجيل التواصل");
        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (SelectedEntry == null) return;
        var confirm = await _dialogService.ShowConfirmAsync("حذف", "هل أنت متأكد؟");
        if (confirm)
        {
            await _dbService.DeleteWaitingListEntryAsync(SelectedEntry.Id);
            await LoadAsync();
        }
    }
}