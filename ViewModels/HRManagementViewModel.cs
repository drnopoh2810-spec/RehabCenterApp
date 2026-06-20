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

public class HRManagementViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    private ObservableCollection<Employee> _employees = new();
    public ObservableCollection<Employee> Employees
    {
        get => _employees;
        set => this.RaiseAndSetIfChanged(ref _employees, value);
    }

    private ObservableCollection<LeaveRequest> _leaveRequests = new();
    public ObservableCollection<LeaveRequest> LeaveRequests
    {
        get => _leaveRequests;
        set => this.RaiseAndSetIfChanged(ref _leaveRequests, value);
    }

    private Employee? _selectedEmployee;
    public Employee? SelectedEmployee
    {
        get => _selectedEmployee;
        set => this.RaiseAndSetIfChanged(ref _selectedEmployee, value);
    }

    private int _selectedTab = 0;
    public int SelectedTab
    {
        get => _selectedTab;
        set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
    }

    private bool _isLeaveFormOpen;
    public bool IsLeaveFormOpen
    {
        get => _isLeaveFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isLeaveFormOpen, value);
    }

    private string _leaveType = "Annual";
    public string LeaveType
    {
        get => _leaveType;
        set => this.RaiseAndSetIfChanged(ref _leaveType, value);
    }

    private DateTime _leaveStart = DateTime.Now;
    public DateTime LeaveStart
    {
        get => _leaveStart;
        set => this.RaiseAndSetIfChanged(ref _leaveStart, value);
    }

    private DateTime _leaveEnd = DateTime.Now.AddDays(1);
    public DateTime LeaveEnd
    {
        get => _leaveEnd;
        set => this.RaiseAndSetIfChanged(ref _leaveEnd, value);
    }

    private string _leaveReason = string.Empty;
    public string LeaveReason
    {
        get => _leaveReason;
        set => this.RaiseAndSetIfChanged(ref _leaveReason, value);
    }

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddEmployeeCommand { get; }
    public ReactiveCommand<Unit, Unit> RequestLeaveCommand { get; }
    public ReactiveCommand<Unit, Unit> ApproveLeaveCommand { get; }
    public ReactiveCommand<Unit, Unit> RejectLeaveCommand { get; }
    public ReactiveCommand<Unit, Unit> EvaluateTherapistCommand { get; }
    public ReactiveCommand<Unit, Unit> PrintPayrollCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public HRManagementViewModel(DatabaseService dbService, DialogService dialogService)
    {
        _dbService = dbService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddEmployeeCommand = ReactiveCommand.Create(() => { });
        RequestLeaveCommand = ReactiveCommand.CreateFromTask(RequestLeaveAsync);
        ApproveLeaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedEmployee != null)
            {
                var leave = LeaveRequests.FirstOrDefault(l => l.EmployeeId == SelectedEmployee.Id && l.Status == "Pending");
                if (leave != null)
                {
                    leave.Status = "Approved";
                    leave.ApprovalDate = DateTime.Now;
                    await _dbService.UpdateLeaveRequestAsync(leave);
                    await LoadAsync();
                }
            }
        });
        RejectLeaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedEmployee != null)
            {
                var leave = LeaveRequests.FirstOrDefault(l => l.EmployeeId == SelectedEmployee.Id && l.Status == "Pending");
                if (leave != null)
                {
                    leave.Status = "Rejected";
                    await _dbService.UpdateLeaveRequestAsync(leave);
                    await LoadAsync();
                }
            }
        });
        EvaluateTherapistCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedEmployee != null)
                await _dialogService.ShowInfoAsync("تقييم", $"تقييم أداء {SelectedEmployee.Name}");
        });
        PrintPayrollCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await _dialogService.ShowInfoAsync("كشف رواتب", "تم طباعة كشف الرواتب");
        });
        CancelCommand = ReactiveCommand.Create(() => { IsLeaveFormOpen = false; });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var empList = await _dbService.GetEmployeesAsync();
        Employees.Clear();
        foreach (var e in empList) Employees.Add(e);

        var leaveList = await _dbService.GetLeaveRequestsAsync();
        LeaveRequests.Clear();
        foreach (var l in leaveList) LeaveRequests.Add(l);
    }

    private async Task RequestLeaveAsync()
    {
        if (SelectedEmployee == null) return;

        var leave = new LeaveRequest
        {
            EmployeeId = SelectedEmployee.Id,
            LeaveType = LeaveType,
            StartDate = LeaveStart,
            EndDate = LeaveEnd,
            Reason = LeaveReason,
            Status = "Pending"
        };

        await _dbService.AddLeaveRequestAsync(leave);
        IsLeaveFormOpen = false;
        await LoadAsync();
        await AuditLogger.LogAsync("Create", "LeaveRequest", $"{SelectedEmployee.Name} requested {LeaveType} leave");
    }
}