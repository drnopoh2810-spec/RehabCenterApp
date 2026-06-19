using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class ParentPortalView : ReactiveUserControl<ParentPortalViewModel>
{
    public ParentPortalView()
    {
        InitializeComponent();
    }
}