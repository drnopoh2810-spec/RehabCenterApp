using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RehabCenterApp.Views;

public partial class UserManagementView : UserControl
{
    public UserManagementView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
