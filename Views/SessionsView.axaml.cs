using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class SessionsView : ReactiveUserControl<SessionsViewModel>
{
    public SessionsView()
    {
        InitializeComponent();
    }
}