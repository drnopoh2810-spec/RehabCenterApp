using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class AccountingView : ReactiveUserControl<AccountingViewModel>
{
    public AccountingView()
    {
        InitializeComponent();
    }
}