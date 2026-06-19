using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class ClinicalReportsView : ReactiveUserControl<ClinicalReportsViewModel>
{
    public ClinicalReportsView()
    {
        InitializeComponent();
    }
}