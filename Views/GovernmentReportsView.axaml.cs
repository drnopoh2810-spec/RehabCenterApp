using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class GovernmentReportsView : ReactiveUserControl<GovernmentReportsViewModel>
{
    public GovernmentReportsView()
    {
        InitializeComponent();
    }
}