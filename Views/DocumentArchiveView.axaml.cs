using Avalonia.Controls;
using Avalonia.ReactiveUI;
using RehabCenterApp.ViewModels;

namespace RehabCenterApp.Views;

public partial class DocumentArchiveView : ReactiveUserControl<DocumentArchiveViewModel>
{
    public DocumentArchiveView()
    {
        InitializeComponent();
    }
}