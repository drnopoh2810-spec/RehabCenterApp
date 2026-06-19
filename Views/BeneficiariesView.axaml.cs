using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class BeneficiariesView : ReactiveUserControl<BeneficiariesViewModel>
{
    public BeneficiariesView()
    {
        InitializeComponent();
    }
}