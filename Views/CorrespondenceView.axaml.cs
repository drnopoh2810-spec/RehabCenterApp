using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class CorrespondenceView : ReactiveUserControl<CorrespondenceViewModel>
{
    public CorrespondenceView()
    {
        InitializeComponent();
    }
}