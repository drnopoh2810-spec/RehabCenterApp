using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class GamificationView : ReactiveUserControl<GamificationViewModel>
{
    public GamificationView()
    {
        InitializeComponent();
    }
}