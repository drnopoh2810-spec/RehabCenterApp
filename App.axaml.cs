using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RehabCenterApp.Views;
using RehabCenterApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace RehabCenterApp;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // ── Shared helper: open MainWindow and close login ────────
            void OpenMainWindow()
            {
                var mainWindow = new MainWindow
                {
                    DataContext = Services.GetRequiredService<ViewModels.MainWindowViewModel>()
                };
                Services.GetRequiredService<DialogService>().SetOwner(mainWindow);
                desktop.MainWindow = mainWindow;
                mainWindow.Show();
                var lw = desktop.Windows.FirstOrDefault(w => w is LoginWindow);
                lw?.Close();
            }

            // ── Helper: build a LoginViewModel ────────────────────────
            ViewModels.LoginViewModel BuildLoginVm() => new ViewModels.LoginViewModel(
                Services.GetRequiredService<DatabaseService>(),
                Services.GetRequiredService<LanAccessService>(),
                onAdminSuccess: OpenMainWindow,
                onTherapistSuccess: (therapistUsername) => OpenTherapistWindow(therapistUsername, desktop),
                onRoleSuccess: (role) => OpenMainWindow()
            );

            var loginVm = BuildLoginVm();
            var loginWindow = new LoginWindow { DataContext = loginVm };
            desktop.MainWindow = loginWindow;
            loginWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OpenTherapistWindow(string therapistUsername, IClassicDesktopStyleApplicationLifetime desktop)
    {
        var therapistVm = new ViewModels.TherapistDashboardViewModel(
            Services.GetRequiredService<DatabaseService>(),
            Services.GetRequiredService<PrintService>(),
            Services.GetRequiredService<WordExportService>(),
            Services.GetRequiredService<DialogService>(),
            therapistUsername,
            onLogout: () =>
            {
                // Re-open login
                var newLoginWindow = new LoginWindow();
                var newLoginVm = new ViewModels.LoginViewModel(
                    Services.GetRequiredService<DatabaseService>(),
                    Services.GetRequiredService<LanAccessService>(),
                    onAdminSuccess: () =>
                    {
                        var mw = new MainWindow
                        {
                            DataContext = Services.GetRequiredService<ViewModels.MainWindowViewModel>()
                        };
                        Services.GetRequiredService<DialogService>().SetOwner(mw);
                        desktop.MainWindow = mw;
                        mw.Show();
                        desktop.Windows.FirstOrDefault(w => w is LoginWindow)?.Close();
                        desktop.Windows.FirstOrDefault(w => w is TherapistWindow)?.Close();
                    },
                    onTherapistSuccess: (u) => OpenTherapistWindow(u, desktop),
                    onRoleSuccess: (role) =>
                    {
                        var mw = new MainWindow
                        {
                            DataContext = Services.GetRequiredService<ViewModels.MainWindowViewModel>()
                        };
                        Services.GetRequiredService<DialogService>().SetOwner(mw);
                        desktop.MainWindow = mw;
                        mw.Show();
                        desktop.Windows.FirstOrDefault(w => w is LoginWindow)?.Close();
                        desktop.Windows.FirstOrDefault(w => w is TherapistWindow)?.Close();
                    }
                );
                newLoginWindow.DataContext = newLoginVm;
                desktop.MainWindow = newLoginWindow;
                newLoginWindow.Show();
                desktop.Windows.FirstOrDefault(w => w is TherapistWindow)?.Close();
            }
        );

        var therapistWindow = new TherapistWindow { DataContext = therapistVm };
        Services.GetRequiredService<DialogService>().SetOwner(therapistWindow);
        desktop.MainWindow = therapistWindow;
        therapistWindow.Show();
        desktop.Windows.FirstOrDefault(w => w is LoginWindow)?.Close();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Database
        services.AddDbContext<AppDbContext>(ServiceLifetime.Singleton);

        // Services
        services.AddSingleton<DatabaseService>();
        services.AddSingleton<DialogService>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<PrintService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<BackupService>();
        services.AddSingleton<LanAccessService>();
        services.AddSingleton<WordExportService>();

        // ViewModels - Core
        services.AddSingleton<ViewModels.MainWindowViewModel>();
        services.AddTransient<ViewModels.LoginViewModel>();
        services.AddTransient<ViewModels.DashboardViewModel>();
        services.AddTransient<ViewModels.BeneficiariesViewModel>();
        services.AddTransient<ViewModels.SessionsViewModel>();
        services.AddTransient<ViewModels.AccountingViewModel>();
        services.AddTransient<ViewModels.CorrespondenceViewModel>();
        services.AddTransient<ViewModels.RemindersViewModel>();
        services.AddTransient<ViewModels.FormsViewModel>();
        services.AddTransient<ViewModels.SettingsViewModel>();
        services.AddTransient<ViewModels.BeneficiaryFormViewModel>();
        services.AddTransient<ViewModels.ReceiptViewModel>();

        // ViewModels - Advanced
        services.AddTransient<ViewModels.AssessmentsViewModel>();
        services.AddTransient<ViewModels.InterventionPlansViewModel>();
        services.AddTransient<ViewModels.WaitingListViewModel>();
        services.AddTransient<ViewModels.InventoryViewModel>();
        services.AddTransient<ViewModels.ParentPortalViewModel>();
        services.AddTransient<ViewModels.MDTMeetingsViewModel>();
        services.AddTransient<ViewModels.AnalyticsViewModel>();
        services.AddTransient<ViewModels.TelehealthViewModel>();
        services.AddTransient<ViewModels.ClinicalReportsViewModel>();
        services.AddTransient<ViewModels.GamificationViewModel>();
        services.AddTransient<ViewModels.GovernmentReportsViewModel>();
        services.AddTransient<ViewModels.DocumentArchiveViewModel>();
        services.AddTransient<ViewModels.HRManagementViewModel>();
        services.AddTransient<ViewModels.TherapistDashboardViewModel>();

        // ViewModels - User Management
        services.AddSingleton<ViewModels.UserManagementViewModel>();
    }
}
