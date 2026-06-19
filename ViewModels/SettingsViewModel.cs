using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;

namespace RehabCenterApp.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly BackupService _backupService;
    private readonly DialogService _dialogService;

    // Center Info
    private string _centerName = string.Empty;
    public string CenterName
    {
        get => _centerName;
        set => this.RaiseAndSetIfChanged(ref _centerName, value);
    }

    private string _centerAddress = string.Empty;
    public string CenterAddress
    {
        get => _centerAddress;
        set => this.RaiseAndSetIfChanged(ref _centerAddress, value);
    }

    private string _centerPhone = string.Empty;
    public string CenterPhone
    {
        get => _centerPhone;
        set => this.RaiseAndSetIfChanged(ref _centerPhone, value);
    }

    private string _centerEmail = string.Empty;
    public string CenterEmail
    {
        get => _centerEmail;
        set => this.RaiseAndSetIfChanged(ref _centerEmail, value);
    }

    // Users
    private ObservableCollection<User> _users = new();
    public ObservableCollection<User> Users
    {
        get => _users;
        set => this.RaiseAndSetIfChanged(ref _users, value);
    }

    private User? _selectedUser;
    public User? SelectedUser
    {
        get => _selectedUser;
        set => this.RaiseAndSetIfChanged(ref _selectedUser, value);
    }

    private string _newUsername = string.Empty;
    public string NewUsername
    {
        get => _newUsername;
        set => this.RaiseAndSetIfChanged(ref _newUsername, value);
    }

    private string _newPassword = string.Empty;
    public string NewPassword
    {
        get => _newPassword;
        set => this.RaiseAndSetIfChanged(ref _newPassword, value);
    }

    private string _newRole = "Receptionist";
    public string NewRole
    {
        get => _newRole;
        set => this.RaiseAndSetIfChanged(ref _newRole, value);
    }

    // Categories
    public ObservableCollection<string> DisabilityTypes { get; } = new();
    public ObservableCollection<string> SessionTypes { get; } = new();
    public ObservableCollection<string> ExpenseCategories { get; } = new();

    private string _newDisabilityType = string.Empty;
    public string NewDisabilityType
    {
        get => _newDisabilityType;
        set => this.RaiseAndSetIfChanged(ref _newDisabilityType, value);
    }

    private string _newSessionType = string.Empty;
    public string NewSessionType
    {
        get => _newSessionType;
        set => this.RaiseAndSetIfChanged(ref _newSessionType, value);
    }

    private string _newExpenseCategory = string.Empty;
    public string NewExpenseCategory
    {
        get => _newExpenseCategory;
        set => this.RaiseAndSetIfChanged(ref _newExpenseCategory, value);
    }

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCenterInfoCommand { get; }
    public ReactiveCommand<Unit, Unit> AddUserCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteUserCommand { get; }
    public ReactiveCommand<Unit, Unit> BackupCommand { get; }
    public ReactiveCommand<Unit, Unit> AddDisabilityTypeCommand { get; }
    public ReactiveCommand<Unit, Unit> AddSessionTypeCommand { get; }
    public ReactiveCommand<Unit, Unit> AddExpenseCategoryCommand { get; }

    public SettingsViewModel(DatabaseService dbService, BackupService backupService, DialogService dialogService)
    {
        _dbService = dbService;
        _backupService = backupService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        SaveCenterInfoCommand = ReactiveCommand.CreateFromTask(SaveCenterInfoAsync);
        AddUserCommand = ReactiveCommand.CreateFromTask(AddUserAsync);
        DeleteUserCommand = ReactiveCommand.CreateFromTask(DeleteUserAsync);
        BackupCommand = ReactiveCommand.CreateFromTask(BackupAsync);
        AddDisabilityTypeCommand = ReactiveCommand.Create(() =>
        {
            if (!string.IsNullOrWhiteSpace(NewDisabilityType))
            {
                DisabilityTypes.Add(NewDisabilityType);
                NewDisabilityType = string.Empty;
            }
        });
        AddSessionTypeCommand = ReactiveCommand.Create(() =>
        {
            if (!string.IsNullOrWhiteSpace(NewSessionType))
            {
                SessionTypes.Add(NewSessionType);
                NewSessionType = string.Empty;
            }
        });
        AddExpenseCategoryCommand = ReactiveCommand.Create(() =>
        {
            if (!string.IsNullOrWhiteSpace(NewExpenseCategory))
            {
                ExpenseCategories.Add(NewExpenseCategory);
                NewExpenseCategory = string.Empty;
            }
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        CenterName = await _dbService.GetSettingAsync("CenterName") ?? "";
        CenterAddress = await _dbService.GetSettingAsync("CenterAddress") ?? "";
        CenterPhone = await _dbService.GetSettingAsync("CenterPhone") ?? "";
        CenterEmail = await _dbService.GetSettingAsync("CenterEmail") ?? "";
    }

    private async Task SaveCenterInfoAsync()
    {
        await _dbService.SetSettingAsync("CenterName", CenterName);
        await _dbService.SetSettingAsync("CenterAddress", CenterAddress);
        await _dbService.SetSettingAsync("CenterPhone", CenterPhone);
        await _dbService.SetSettingAsync("CenterEmail", CenterEmail);
        await _dialogService.ShowInfoAsync("Success", "Center information saved successfully!");
    }

    private async Task AddUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword))
            return;

        // Add user logic (requires password hashing)
        NewUsername = string.Empty;
        NewPassword = string.Empty;
    }

    private async Task DeleteUserAsync()
    {
        if (SelectedUser != null)
        {
            // Delete user logic
        }
    }

    private async Task BackupAsync()
    {
        var path = await _backupService.CreateBackupAsync();
        await _dialogService.ShowInfoAsync("Backup", $"Backup created at: {path}");
    }
}