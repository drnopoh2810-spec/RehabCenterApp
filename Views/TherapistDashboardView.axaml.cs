using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class TherapistDashboardView : ReactiveUserControl<TherapistDashboardViewModel>
{
    public TherapistDashboardView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
