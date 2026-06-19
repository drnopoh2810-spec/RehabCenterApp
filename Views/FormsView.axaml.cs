using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class FormsView : ReactiveUserControl<FormsViewModel>
{
    public FormsView()
    {
        InitializeComponent();
    }
}