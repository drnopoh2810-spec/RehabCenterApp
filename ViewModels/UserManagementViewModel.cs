using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class UserManagementViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;
    private readonly LanAccessService _lanAccessService;

    private List<User> _allUsers = new();

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

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { this.RaiseAndSetIfChanged(ref _searchText, value); FilterUsers(); }
    }

    // ─── Form fields ──────────────────────────────────────────────
    private bool _isFormOpen;
    public bool IsFormOpen
    {
        get => _isFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isFormOpen, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => this.RaiseAndSetIfChanged(ref _isEditMode, value);
    }

    private string _formTitle = "إضافة مستخدم";
    public string FormTitle
    {
        get => _formTitle;
        set => this.RaiseAndSetIfChanged(ref _formTitle, value);
    }

    private string _fUsername = string.Empty;
    public string FUsername { get => _fUsername; set => this.RaiseAndSetIfChanged(ref _fUsername, value); }

    private string _fFullName = string.Empty;
    public string FFullName { get => _fFullName; set => this.RaiseAndSetIfChanged(ref _fFullName, value); }

    private string _fRole = "Receptionist";
    public string FRole { get => _fRole; set => this.RaiseAndSetIfChanged(ref _fRole, value); }

    private string _fPhone = string.Empty;
    public string FPhone { get => _fPhone; set => this.RaiseAndSetIfChanged(ref _fPhone, value); }

    private string _fEmail = string.Empty;
    public string FEmail { get => _fEmail; set => this.RaiseAndSetIfChanged(ref _fEmail, value); }

    private string _fPassword = string.Empty;
    public string FPassword { get => _fPassword; set => this.RaiseAndSetIfChanged(ref _fPassword, value); }

    private string _fPasswordConfirm = string.Empty;
    public string FPasswordConfirm { get => _fPasswordConfirm; set => this.RaiseAndSetIfChanged(ref _fPasswordConfirm, value); }

    private bool _fRequireLan = true;
    public bool FRequireLan { get => _fRequireLan; set => this.RaiseAndSetIfChanged(ref _fRequireLan, value); }

    private bool _fIsActive = true;
    public bool FIsActive { get => _fIsActive; set => this.RaiseAndSetIfChanged(ref _fIsActive, value); }

    private string _fNotes = string.Empty;
    public string FNotes { get => _fNotes; set => this.RaiseAndSetIfChanged(ref _fNotes, value); }

    // ─── LAN / Network info ───────────────────────────────────────
    private string _currentIp = string.Empty;
    public string CurrentIp { get => _currentIp; set => this.RaiseAndSetIfChanged(ref _currentIp, value); }

    private string _allowedSubnet = string.Empty;
    public string AllowedSubnet { get => _allowedSubnet; set => this.RaiseAndSetIfChanged(ref _allowedSubnet, value); }

    private string _lanStatusText = string.Empty;
    public string LanStatusText { get => _lanStatusText; set => this.RaiseAndSetIfChanged(ref _lanStatusText, value); }

    // ─── Role descriptions ────────────────────────────────────────
    public List<RoleInfo> Roles { get; } = new()
    {
        new("Admin",       "مدير النظام",       "وصول كامل لجميع الأقسام والإعدادات وإدارة المستخدمين"),
        new("Receptionist","موظف استقبال",      "المستفيدون، قائمة الانتظار، الجلسات، المراسلات، التذكيرات"),
        new("Accountant",  "محاسب",             "المحاسبة، المخزون، التقارير المالية، الرواتب"),
        new("HR",          "موارد بشرية",       "إدارة الموظفين، الرواتب، الحضور والغياب، الإجازات"),
        new("PR",          "علاقات عامة",       "المراسلات، بوابة الأولياء، النماذج، الأرشيف، الذكريات"),
        new("Therapist",   "أخصائي / معالج",    "بوابة الأخصائي الخاصة (جلساته وتقاريره فقط)"),
    };

    private RoleInfo? _selectedRoleInfo;
    public RoleInfo? SelectedRoleInfo
    {
        get => _selectedRoleInfo;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedRoleInfo, value);
            if (value != null) FRole = value.Key;
        }
    }

    // ─── Commands ─────────────────────────────────────────────────
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddUserCommand { get; }
    public ReactiveCommand<Unit, Unit> EditUserCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveUserCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleActiveCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteUserCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveSubnetCommand { get; }
    public ReactiveCommand<Unit, Unit> DetectSubnetCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public UserManagementViewModel(DatabaseService dbService, DialogService dialogService, LanAccessService lanAccessService)
    {
        _dbService = dbService;
        _dialogService = dialogService;
        _lanAccessService = lanAccessService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddUserCommand = ReactiveCommand.Create(OpenAddForm);
        EditUserCommand = ReactiveCommand.Create(OpenEditForm);
        SaveUserCommand = ReactiveCommand.CreateFromTask(SaveUserAsync);
        ToggleActiveCommand = ReactiveCommand.CreateFromTask(ToggleActiveAsync);
        DeleteUserCommand = ReactiveCommand.CreateFromTask(DeleteUserAsync);
        SaveSubnetCommand = ReactiveCommand.CreateFromTask(SaveSubnetAsync);
        DetectSubnetCommand = ReactiveCommand.Create(DetectSubnet);
        CancelCommand = ReactiveCommand.Create(() => { IsFormOpen = false; });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        _allUsers = await _dbService.GetUsersAsync();
        FilterUsers();

        CurrentIp = _lanAccessService.GetPrimaryLocalIp();
        AllowedSubnet = await _lanAccessService.GetAllowedSubnetAsync();
        UpdateLanStatus();
    }

    private void FilterUsers()
    {
        Users.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allUsers
            : _allUsers.Where(u =>
                u.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (u.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                u.Role.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        foreach (var u in filtered) Users.Add(u);
    }

    private void UpdateLanStatus()
    {
        var ok = _lanAccessService.IsAllowedAccess(AllowedSubnet);
        LanStatusText = ok
            ? $"✓ متصل بالشبكة المسموح بها  ({CurrentIp})"
            : $"✗ IP الحالي ({CurrentIp}) خارج النطاق ({AllowedSubnet})";
    }

    private void DetectSubnet()
    {
        var ip = _lanAccessService.GetPrimaryLocalIp();
        if (ip != "127.0.0.1")
        {
            var parts = ip.Split('.');
            if (parts.Length >= 3)
                AllowedSubnet = string.Join(".", parts[0], parts[1], parts[2]) + ".";
        }
        CurrentIp = ip;
        UpdateLanStatus();
    }

    private async Task SaveSubnetAsync()
    {
        await _dbService.SetSettingAsync("AllowedSubnet", AllowedSubnet);
        UpdateLanStatus();
        await _dialogService.ShowInfoAsync("تم الحفظ", $"تم تحديث نطاق الشبكة المسموح بها إلى: {AllowedSubnet}");
    }

    // ─── Form ─────────────────────────────────────────────────────
    private void OpenAddForm()
    {
        IsEditMode = false;
        FormTitle = "إضافة مستخدم جديد";
        FUsername = FFullName = FPhone = FEmail = FPassword = FPasswordConfirm = FNotes = string.Empty;
        FRole = "Receptionist";
        FRequireLan = true;
        FIsActive = true;
        SelectedRoleInfo = Roles.FirstOrDefault(r => r.Key == FRole);
        IsFormOpen = true;
    }

    private void OpenEditForm()
    {
        if (SelectedUser == null) return;
        IsEditMode = true;
        FormTitle = "تعديل بيانات المستخدم";
        var u = SelectedUser;
        FUsername = u.Username;
        FFullName = u.FullName ?? "";
        FRole = u.Role;
        FPhone = u.Phone ?? "";
        FEmail = u.Email ?? "";
        FNotes = u.Notes ?? "";
        FRequireLan = u.RequireLanAccess;
        FIsActive = u.IsActive;
        FPassword = FPasswordConfirm = string.Empty;
        SelectedRoleInfo = Roles.FirstOrDefault(r => r.Key == FRole);
        IsFormOpen = true;
    }

    private async Task SaveUserAsync()
    {
        if (string.IsNullOrWhiteSpace(FUsername))
        {
            await _dialogService.ShowInfoAsync("خطأ", "اسم المستخدم مطلوب");
            return;
        }

        if (!IsEditMode && string.IsNullOrWhiteSpace(FPassword))
        {
            await _dialogService.ShowInfoAsync("خطأ", "كلمة المرور مطلوبة لإنشاء مستخدم جديد");
            return;
        }

        if (!string.IsNullOrEmpty(FPassword) && FPassword != FPasswordConfirm)
        {
            await _dialogService.ShowInfoAsync("خطأ", "كلمة المرور وتأكيدها غير متطابقتين");
            return;
        }

        if (IsEditMode && SelectedUser != null)
        {
            var u = SelectedUser;
            u.Username = FUsername;
            u.FullName = FFullName;
            u.Role = FRole;
            u.Phone = FPhone;
            u.Email = FEmail;
            u.Notes = FNotes;
            u.RequireLanAccess = FRequireLan;
            u.IsActive = FIsActive;
            if (!string.IsNullOrEmpty(FPassword))
                u.PasswordHash = PasswordHasher.HashPassword(FPassword);
            await _dbService.UpdateUserAsync(u);
            await AuditLogger.LogAsync("Update", "User", $"تعديل مستخدم {u.Username}");
        }
        else
        {
            // Check username not duplicate
            var existing = _allUsers.FirstOrDefault(u => u.Username == FUsername);
            if (existing != null)
            {
                await _dialogService.ShowInfoAsync("خطأ", "اسم المستخدم موجود مسبقاً");
                return;
            }

            var newUser = new User
            {
                Username = FUsername,
                FullName = FFullName,
                Role = FRole,
                Phone = FPhone,
                Email = FEmail,
                Notes = FNotes,
                RequireLanAccess = FRequireLan,
                IsActive = FIsActive,
                PasswordHash = PasswordHasher.HashPassword(FPassword)
            };
            await _dbService.AddUserAsync(newUser);
            await AuditLogger.LogAsync("Create", "User", $"إنشاء مستخدم {newUser.Username} بدور {newUser.Role}");
        }

        IsFormOpen = false;
        await LoadAsync();
    }

    private async Task ToggleActiveAsync()
    {
        if (SelectedUser == null) return;
        if (SelectedUser.Role == "Admin" && SelectedUser.Username == "admin")
        {
            await _dialogService.ShowInfoAsync("تنبيه", "لا يمكن تعطيل حساب الأدمن الرئيسي");
            return;
        }
        SelectedUser.IsActive = !SelectedUser.IsActive;
        await _dbService.UpdateUserAsync(SelectedUser);
        await LoadAsync();
    }

    private async Task DeleteUserAsync()
    {
        if (SelectedUser == null) return;
        if (SelectedUser.Username == "admin")
        {
            await _dialogService.ShowInfoAsync("تنبيه", "لا يمكن حذف حساب الأدمن الرئيسي");
            return;
        }
        var confirmed = await _dialogService.ShowConfirmAsync("تأكيد الحذف", $"هل تريد حذف المستخدم '{SelectedUser.Username}'؟");
        if (!confirmed) return;
        await _dbService.DeleteUserAsync(SelectedUser.Id);
        await AuditLogger.LogAsync("Delete", "User", $"حذف مستخدم {SelectedUser.Username}");
        await LoadAsync();
    }
}

public record RoleInfo(string Key, string DisplayName, string Description);
