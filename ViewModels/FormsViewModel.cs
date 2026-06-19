using System;
using System.Reactive;
using ReactiveUI;
using RehabCenterApp.Services;

namespace RehabCenterApp.ViewModels;

public class FormsViewModel : ViewModelBase
{
    private readonly PrintService _printService;

    private int _selectedFormIndex = 0;
    public int SelectedFormIndex
    {
        get => _selectedFormIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedFormIndex, value);
    }

    public ReactiveCommand<Unit, Unit> PrintCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportPdfCommand { get; }

    public FormsViewModel(PrintService printService)
    {
        _printService = printService;
        PrintCommand = ReactiveCommand.Create(() => { });
        ExportPdfCommand = ReactiveCommand.Create(() => { });
    }
}