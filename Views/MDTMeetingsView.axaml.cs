using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class MDTMeetingsView : ReactiveUserControl<MDTMeetingsViewModel>
{
    public MDTMeetingsView()
    {
        InitializeComponent();
    }
}