using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly LanAccessService _lanAccessService;
    private readonly Action _onAdminSuccess;
    private readonly Action<string> _onTherapistSuccess;
    private readonly Action<string> _onRoleSuccess;

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    private bool _hasError;
    public bool HasError
    {
        get => _hasError;
        set => this.RaiseAndSetIfChanged(ref _hasError, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleLangCommand { get; }

    /// <param name="onAdminSuccess">Opens admin/MainWindow</param>
    /// <param name="onTherapistSuccess">Opens TherapistWindow with username</param>
    /// <param name="onRoleSuccess">Opens MainWindow for non-admin roles (role-based nav)</param>
    public LoginViewModel(
        DatabaseService dbService,
        LanAccessService lanAccessService,
        Action onAdminSuccess,
        Action<string> onTherapistSuccess,
        Action<string>? onRoleSuccess = null)
    {
        _dbService = dbService;
        _lanAccessService = lanAccessService;
        _onAdminSuccess = onAdminSuccess;
        _onTherapistSuccess = onTherapistSuccess;
        _onRoleSuccess = onRoleSuccess ?? (role => onAdminSuccess());

        LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
        ToggleLangCommand = ReactiveCommand.Create(() => LocalizationService.Instance.ToggleLanguage());
    }

    private async Task LoginAsync()
    {
        HasError = false;
        IsLoading = true;

        try
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "الرجاء إدخال اسم المستخدم وكلمة المرور";
                HasError = true;
                return;
            }

            var user = await _dbService.GetUserByUsernameAsync(Username.Trim());
            if (user == null)
            {
                ErrorMessage = "اسم المستخدم أو كلمة المرور غير صحيحة";
                HasError = true;
                return;
            }

            if (!PasswordHasher.VerifyPassword(Password, user.PasswordHash))
            {
                ErrorMessage = "اسم المستخدم أو كلمة المرور غير صحيحة";
                HasError = true;
                return;
            }

            if (!user.IsActive)
            {
                ErrorMessage = "هذا الحساب معطّل. الرجاء التواصل مع مدير النظام.";
                HasError = true;
                return;
            }

            // ── LAN check for all roles except Admin ─────────────────
            if (user.Role != "Admin" && user.RequireLanAccess)
            {
                var allowedSubnet = await _lanAccessService.GetAllowedSubnetAsync();
                if (!string.IsNullOrWhiteSpace(allowedSubnet) && !_lanAccessService.IsAllowedAccess(allowedSubnet))
                {
                    var localIp = _lanAccessService.GetPrimaryLocalIp();
                    ErrorMessage = $"الوصول مرفوض — يُسمح فقط للمتصلين بشبكة: {allowedSubnet}*\nعنوان IP الحالي: {localIp}\n\nالرجاء الاتصال بنفس شبكة الواي فاي الخاصة بالمركز.";
                    HasError = true;
                    return;
                }
            }

            // ── Therapist portal check ────────────────────────────────
            if (user.Role == "Therapist")
            {
                var portalEnabled = await _dbService.GetSettingAsync("TherapistPortalEnabled") ?? "True";
                if (portalEnabled != "True")
                {
                    ErrorMessage = "بوابة المعالج غير مفعلة. الرجاء التواصل مع المشرف.";
                    HasError = true;
                    return;
                }
            }

            // ── Save session ──────────────────────────────────────────
            UserSessionService.Instance.SetUser(user);
            await _dbService.SetSettingAsync("CurrentUser", user.Username);
            await _dbService.SetSettingAsync("CurrentRole", user.Role);

            // Update last login
            user.LastLogin = DateTime.Now;
            await _dbService.UpdateUserAsync(user);

            await AuditLogger.LogAsync("Login", "User", $"تسجيل دخول: {user.Username} ({user.Role})");

            // ── Route by role ─────────────────────────────────────────
            if (user.Role == "Therapist")
                _onTherapistSuccess(user.Username);
            else if (user.Role == "Admin")
                _onAdminSuccess();
            else
                _onRoleSuccess(user.Role);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
