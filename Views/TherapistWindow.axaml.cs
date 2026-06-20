using Avalonia.Markup.Xaml;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class TherapistWindow : Avalonia.Controls.Window
{
    public TherapistWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
