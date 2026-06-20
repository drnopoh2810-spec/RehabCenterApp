using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace RehabCenterApp.Services;

public class DialogService
{
    private Window? _owner;

    public void SetOwner(Window owner)
    {
        _owner = owner;
    }

    public async Task<bool> ShowConfirmAsync(string title, string message)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandard(
            title,
            message,
            ButtonEnum.YesNo,
            Icon.Question);

        var result = _owner != null
            ? await msgBox.ShowWindowDialogAsync(_owner)
            : await msgBox.ShowAsync();

        return result == ButtonResult.Yes;
    }

    public async Task ShowInfoAsync(string title, string message)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandard(
            title,
            message,
            ButtonEnum.Ok,
            Icon.Info);

        if (_owner != null)
            await msgBox.ShowWindowDialogAsync(_owner);
        else
            await msgBox.ShowAsync();
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandard(
            title,
            message,
            ButtonEnum.Ok,
            Icon.Error);

        if (_owner != null)
            await msgBox.ShowWindowDialogAsync(_owner);
        else
            await msgBox.ShowAsync();
    }
}
