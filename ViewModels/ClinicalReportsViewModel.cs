using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class ClinicalReportsViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly PrintService _printService;
    private readonly DialogService _dialogService;

    private ObservableCollection<ClinicalReport> _reports = new();
    public ObservableCollection<ClinicalReport> Reports
    {
        get => _reports;
        set => this.RaiseAndSetIfChanged(ref _reports, value);
    }

    private ClinicalReport? _selectedReport;
    public ClinicalReport? SelectedReport
    {
        get => _selectedReport;
        set => this.RaiseAndSetIfChanged(ref _selectedReport, value);
    }

    private bool _isFormOpen;
    public bool IsFormOpen
    {
        get => _isFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isFormOpen, value);
    }

    private Beneficiary? _selectedBeneficiary;
    public Beneficiary? SelectedBeneficiary
    {
        get => _selectedBeneficiary;
        set => this.RaiseAndSetIfChanged(ref _selectedBeneficiary, value);
    }

    private string _reportType = "Initial";
    public string ReportType
    {
        get => _reportType;
        set => this.RaiseAndSetIfChanged(ref _reportType, value);
    }

    private string _content = string.Empty;
    public string Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    public ObservableCollection<Beneficiary> Beneficiaries { get; } = new();
    public ObservableCollection<Employee> Therapists { get; } = new();

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> ApproveCommand { get; }
    public ReactiveCommand<Unit, Unit> PrintCommand { get; }
    public ReactiveCommand<Unit, Unit> SignDigitallyCommand { get; }

    public ClinicalReportsViewModel(DatabaseService dbService, PrintService printService, DialogService dialogService)
    {
        _dbService = dbService;
        _printService = printService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var list = await _dbService.GetBeneficiariesAsync();
            Beneficiaries.Clear();
            foreach (var b in list) Beneficiaries.Add(b);
            IsFormOpen = true;
        });
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(() => IsFormOpen = false);
        ApproveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedReport != null)
            {
                SelectedReport.Status = "Approved";
                SelectedReport.ApprovalDate = DateTime.Now;
                await _dbService.UpdateClinicalReportAsync(SelectedReport);
                await LoadAsync();
            }
        });
        PrintCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedReport != null)
                await _dialogService.ShowInfoAsync("طباعة", "تم طباعة التقرير السريري");
        });
        SignDigitallyCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedReport != null)
            {
                SelectedReport.DigitalSignature = $"SIG-{Guid.NewGuid().ToString("N")[..8]}";
                SelectedReport.Status = "Signed";
                await _dbService.UpdateClinicalReportAsync(SelectedReport);
                await LoadAsync();
                await _dialogService.ShowInfoAsync("توقيع", "تم التوقيع الرقمي بنجاح");
            }
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _dbService.GetClinicalReportsAsync();
        Reports.Clear();
        foreach (var r in list) Reports.Add(r);
    }

    private async Task SaveAsync()
    {
        if (SelectedBeneficiary == null || string.IsNullOrWhiteSpace(Content)) return;

        var report = new ClinicalReport
        {
            BeneficiaryId = SelectedBeneficiary.Id,
            ReportType = ReportType,
            Content = Content,
            ReportDate = DateTime.Now,
            Status = "Draft"
        };

        await _dbService.AddClinicalReportAsync(report);
        IsFormOpen = false;
        Content = string.Empty;
        await LoadAsync();
        await AuditLogger.LogAsync("Create", "ClinicalReport", $"Created {ReportType} report for {SelectedBeneficiary.Name}");
    }
}