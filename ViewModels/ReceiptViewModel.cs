using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;

namespace RehabCenterApp.ViewModels;

public class ReceiptViewModel : ViewModelBase
{
    private readonly PrintService _printService;
    private readonly DatabaseService _dbService;

    private Payment? _payment;
    public Payment? Payment
    {
        get => _payment;
        set => this.RaiseAndSetIfChanged(ref _payment, value);
    }

    private string _centerName = string.Empty;
    public string CenterName
    {
        get => _centerName;
        set => this.RaiseAndSetIfChanged(ref _centerName, value);
    }

    private string _centerPhone = string.Empty;
    public string CenterPhone
    {
        get => _centerPhone;
        set => this.RaiseAndSetIfChanged(ref _centerPhone, value);
    }

    public ReactiveCommand<Unit, Unit> PrintCommand { get; }

    public ReceiptViewModel(PrintService printService, DatabaseService dbService)
    {
        _printService = printService;
        _dbService = dbService;
        PrintCommand = ReactiveCommand.CreateFromTask(PrintAsync);
        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        CenterName = await _dbService.GetSettingAsync("CenterName") ?? "Rehab Center";
        CenterPhone = await _dbService.GetSettingAsync("CenterPhone") ?? "";
    }

    private async Task PrintAsync()
    {
        if (Payment != null)
        {
            var path = await _printService.GenerateReceiptAsync(Payment, CenterName, CenterPhone);
            // Open PDF or print
        }
    }
}