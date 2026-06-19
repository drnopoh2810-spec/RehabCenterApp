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
            var loginVm = new ViewModels.LoginViewModel(
                Services.GetRequiredService<DatabaseService>(),
                () =>
                {
                    var mainWindow = new MainWindow
                    {
                        DataContext = Services.GetRequiredService<ViewModels.MainWindowViewModel>()
                    };

                    Services.GetRequiredService<DialogService>().SetOwner(mainWindow);

                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();

                    var loginWindow = desktop.Windows.FirstOrDefault(w => w is LoginWindow);
                    loginWindow?.Close();
                });

            var loginWindow = new LoginWindow { DataContext = loginVm };
            desktop.MainWindow = loginWindow;
            loginWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
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
    }
}