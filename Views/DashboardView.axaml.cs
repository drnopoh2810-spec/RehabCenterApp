using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class DashboardView : ReactiveUserControl<DashboardViewModel>
{
    public DashboardView()
    {
        InitializeComponent();
    }
}