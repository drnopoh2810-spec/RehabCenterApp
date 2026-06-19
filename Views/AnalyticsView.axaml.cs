using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class AnalyticsView : ReactiveUserControl<AnalyticsViewModel>
{
    public AnalyticsView()
    {
        InitializeComponent();
    }
}