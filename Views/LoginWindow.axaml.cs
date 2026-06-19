using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class LoginWindow : ReactiveWindow<LoginViewModel>
{
    public LoginWindow()
    {
        InitializeComponent();
    }
}