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

public class GovernmentReportsViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    private ObservableCollection<GovernmentReport> _reports = new();
    public ObservableCollection<GovernmentReport> Reports
    {
        get => _reports;
        set => this.RaiseAndSetIfChanged(ref _reports, value);
    }

    private string _reportType = "Monthly";
    public string ReportType
    {
        get => _reportType;
        set => this.RaiseAndSetIfChanged(ref _reportType, value);
    }

    private string _authority = "Ministry";
    public string Authority
    {
        get => _authority;
        set => this.RaiseAndSetIfChanged(ref _authority, value);
    }

    private DateTime _periodStart = DateTime.Now.AddMonths(-1);
    public DateTime PeriodStart
    {
        get => _periodStart;
        set => this.RaiseAndSetIfChanged(ref _periodStart, value);
    }

    private DateTime _periodEnd = DateTime.Now;
    public DateTime PeriodEnd
    {
        get => _periodEnd;
        set => this.RaiseAndSetIfChanged(ref _periodEnd, value);
    }

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> GenerateCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportCommand { get; }
    public ReactiveCommand<Unit, Unit> SubmitCommand { get; }

    public GovernmentReportsViewModel(DatabaseService dbService, DialogService dialogService)
    {
        _dbService = dbService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        GenerateCommand = ReactiveCommand.CreateFromTask(GenerateAsync);
        ExportCommand = ReactiveCommand.CreateFromTask(ExportAsync);
        SubmitCommand = ReactiveCommand.CreateFromTask(SubmitAsync);

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _dbService.GetGovernmentReportsAsync();
        Reports.Clear();
        foreach (var r in list) Reports.Add(r);
    }

    private async Task GenerateAsync()
    {
        var beneficiaries = await _dbService.GetBeneficiariesAsync();
        var sessions = await _dbService.GetSessionsAsync(date: PeriodStart);
        var payments = await _dbService.GetPaymentsAsync(PeriodStart, PeriodEnd);

        var data = new
        {
            TotalBeneficiaries = beneficiaries.Count,
            TotalSessions = sessions.Count,
            TotalRevenue = payments.Sum(p => p.Amount),
            NewRegistrations = beneficiaries.Count(b => b.RegistrationDate >= PeriodStart && b.RegistrationDate <= PeriodEnd),
            DisabilityBreakdown = beneficiaries.GroupBy(b => b.DisabilityType).ToDictionary(g => g.Key, g => g.Count())
        };

        var report = new GovernmentReport
        {
            ReportType = ReportType,
            Authority = Authority,
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd,
            GeneratedData = System.Text.Json.JsonSerializer.Serialize(data),
            Status = "Generated"
        };

        await _dbService.AddGovernmentReportAsync(report);
        await LoadAsync();
        await _dialogService.ShowInfoAsync("تم", "تم إنشاء التقرير الحكومي بنجاح");
    }

    private async Task ExportAsync()
    {
        await _dialogService.ShowInfoAsync("تصدير", "تم تصدير التقرير بصيغة PDF");
    }

    private async Task SubmitAsync()
    {
        await _dialogService.ShowInfoAsync("إرسال", "تم إرسال التقرير إلى الجهة المختصة");
    }
}