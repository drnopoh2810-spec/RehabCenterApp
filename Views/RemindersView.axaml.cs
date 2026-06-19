using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class RemindersView : ReactiveUserControl<RemindersViewModel>
{
    public RemindersView()
    {
        InitializeComponent();
    }
}