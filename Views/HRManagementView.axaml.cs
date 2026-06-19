using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class HRManagementView : ReactiveUserControl<HRManagementViewModel>
{
    public HRManagementView()
    {
        InitializeComponent();
    }
}