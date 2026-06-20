using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class ParentPortalViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    private ObservableCollection<ParentCommunication> _communications = new();
    public ObservableCollection<ParentCommunication> Communications
    {
        get => _communications;
        set => this.RaiseAndSetIfChanged(ref _communications, value);
    }

    private ParentCommunication? _selectedCommunication;
    public ParentCommunication? SelectedCommunication
    {
        get => _selectedCommunication;
        set => this.RaiseAndSetIfChanged(ref _selectedCommunication, value);
    }

    private Beneficiary? _selectedBeneficiary;
    public Beneficiary? SelectedBeneficiary
    {
        get => _selectedBeneficiary;
        set => this.RaiseAndSetIfChanged(ref _selectedBeneficiary, value);
    }

    private bool _isFormOpen;
    public bool IsFormOpen
    {
        get => _isFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isFormOpen, value);
    }

    private string _messageContent = string.Empty;
    public string MessageContent
    {
        get => _messageContent;
        set => this.RaiseAndSetIfChanged(ref _messageContent, value);
    }

    private string _communicationType = "WhatsApp";
    public string CommunicationType
    {
        get => _communicationType;
        set => this.RaiseAndSetIfChanged(ref _communicationType, value);
    }

    public ObservableCollection<Beneficiary> Beneficiaries { get; } = new();

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> SendCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> SendAutoReportCommand { get; }

    public ParentPortalViewModel(DatabaseService dbService, DialogService dialogService)
    {
        _dbService = dbService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var list = await _dbService.GetBeneficiariesAsync();
            Beneficiaries.Clear();
            foreach (var b in list) Beneficiaries.Add(b);
            IsFormOpen = true;
        });
        SendCommand = ReactiveCommand.CreateFromTask(SendAsync);
        CancelCommand = ReactiveCommand.Create(() => { IsFormOpen = false; });
        SendAutoReportCommand = ReactiveCommand.CreateFromTask(SendAutoReportAsync);

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _dbService.GetParentCommunicationsAsync();
        Communications.Clear();
        foreach (var c in list) Communications.Add(c);
    }

    private async Task SendAsync()
    {
        if (SelectedBeneficiary == null || string.IsNullOrWhiteSpace(MessageContent))
            return;

        var comm = new ParentCommunication
        {
            BeneficiaryId = SelectedBeneficiary.Id,
            CommunicationType = CommunicationType,
            Content = MessageContent,
            Direction = "Out",
            Date = DateTime.Now,
            ContactPerson = SelectedBeneficiary.GuardianName,
            PhoneNumber = SelectedBeneficiary.GuardianPhone,
            Status = "Sent"
        };

        await _dbService.AddParentCommunicationAsync(comm);

        // Simulate WhatsApp/SMS sending
        if (CommunicationType == "WhatsApp" || CommunicationType == "SMS")
        {
            await _dialogService.ShowInfoAsync("إرسال", 
                $"تم إرسال الرسالة إلى {SelectedBeneficiary.GuardianPhone}");
        }

        IsFormOpen = false;
        MessageContent = string.Empty;
        await LoadAsync();
        await AuditLogger.LogAsync("Send", "ParentCommunication", 
            $"Sent {CommunicationType} to {SelectedBeneficiary.Name}");
    }

    private async Task SendAutoReportAsync()
    {
        if (SelectedBeneficiary == null) return;

        var sessions = await _dbService.GetSessionsAsync(beneficiaryId: SelectedBeneficiary.Id);
        var payments = await _dbService.GetPaymentsAsync(beneficiaryId: SelectedBeneficiary.Id);

        var report = $"تقرير تلقائي - {SelectedBeneficiary.Name}\n" +
                     $"الجلسات: {sessions.Count}\n" +
                     $"المدفوعات: {payments.Sum(p => p.Amount):C}\n" +
                     $"تاريخ: {DateTime.Now:yyyy-MM-dd}";

        var comm = new ParentCommunication
        {
            BeneficiaryId = SelectedBeneficiary.Id,
            CommunicationType = "WhatsApp",
            Content = report,
            Direction = "Out",
            Date = DateTime.Now,
            IsReport = true,
            Status = "Sent"
        };

        await _dbService.AddParentCommunicationAsync(comm);
        await _dialogService.ShowInfoAsync("تقرير", "تم إرسال التقرير التلقائي");
        await LoadAsync();
    }
}