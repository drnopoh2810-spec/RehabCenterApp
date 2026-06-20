using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RehabCenterApp.Views;

public partial class BeneficiaryProfileView : UserControl
{
    public BeneficiaryProfileView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
