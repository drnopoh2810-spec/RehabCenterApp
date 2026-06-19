using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class InventoryView : ReactiveUserControl<InventoryViewModel>
{
    public InventoryView()
    {
        InitializeComponent();
    }
}