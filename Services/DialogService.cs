using System.Threading.Tasks;
using Avalonia.Controls;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.DTO;

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
        var msgBox = MessageBoxManager.GetMessageBoxStandardWindow(
            new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.YesNo,
                ContentTitle = title,
                ContentMessage = message,
                Icon = Icon.Question,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });

        var result = _owner != null 
            ? await msgBox.ShowDialog(_owner)
            : await msgBox.Show();

        return result == ButtonResult.Yes;
    }

    public async Task ShowInfoAsync(string title, string message)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandardWindow(
            new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = message,
                Icon = Icon.Info,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });

        if (_owner != null)
            await msgBox.ShowDialog(_owner);
        else
            await msgBox.Show();
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandardWindow(
            new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = message,
                Icon = Icon.Error,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });

        if (_owner != null)
            await msgBox.ShowDialog(_owner);
        else
            await msgBox.Show();
    }
}