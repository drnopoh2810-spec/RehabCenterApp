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
    private readonly Action _onLoginSuccess;

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

    public LoginViewModel(DatabaseService dbService, Action onLoginSuccess)
    {
        _dbService = dbService;
        _onLoginSuccess = onLoginSuccess;
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

            await _dbService.SetSettingAsync("CurrentUser", user.Username);
            await _dbService.SetSettingAsync("CurrentRole", user.Role);
            await AuditLogger.LogAsync("Login", "User", $"User {user.Username} logged in");

            _onLoginSuccess();
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