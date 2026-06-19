using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class TelehealthView : ReactiveUserControl<TelehealthViewModel>
{
    public TelehealthView()
    {
        InitializeComponent();
    }
}