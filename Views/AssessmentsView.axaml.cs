using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class AssessmentsView : ReactiveUserControl<AssessmentsViewModel>
{
    public AssessmentsView()
    {
        InitializeComponent();
    }
}