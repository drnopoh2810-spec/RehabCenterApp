using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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

    private List<Employee> _allEmployees = new();

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

    private ObservableCollection<EmployeePayroll> _payrolls = new();
    public ObservableCollection<EmployeePayroll> Payrolls
    {
        get => _payrolls;
        set => this.RaiseAndSetIfChanged(ref _payrolls, value);
    }

    private ObservableCollection<EmployeeAttendance> _attendanceRecords = new();
    public ObservableCollection<EmployeeAttendance> AttendanceRecords
    {
        get => _attendanceRecords;
        set => this.RaiseAndSetIfChanged(ref _attendanceRecords, value);
    }

    private Employee? _selectedEmployee;
    public Employee? SelectedEmployee
    {
        get => _selectedEmployee;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedEmployee, value);
            if (value != null)
                _ = LoadEmployeeDetailsAsync(value);
        }
    }

    private EmployeePayroll? _selectedPayroll;
    public EmployeePayroll? SelectedPayroll
    {
        get => _selectedPayroll;
        set => this.RaiseAndSetIfChanged(ref _selectedPayroll, value);
    }

    private int _selectedTab = 0;
    public int SelectedTab
    {
        get => _selectedTab;
        set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterEmployees();
        }
    }

    // ─── Employee Form ───────────────────────────────────────────
    private bool _isEmployeeFormOpen;
    public bool IsEmployeeFormOpen
    {
        get => _isEmployeeFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isEmployeeFormOpen, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => this.RaiseAndSetIfChanged(ref _isEditMode, value);
    }

    private string _formTitle = "إضافة موظف";
    public string FormTitle
    {
        get => _formTitle;
        set => this.RaiseAndSetIfChanged(ref _formTitle, value);
    }

    private string _empNumber = string.Empty;
    public string EmpNumber { get => _empNumber; set => this.RaiseAndSetIfChanged(ref _empNumber, value); }
    private string _empName = string.Empty;
    public string EmpName { get => _empName; set => this.RaiseAndSetIfChanged(ref _empName, value); }
    private string _empRole = "Therapist";
    public string EmpRole { get => _empRole; set => this.RaiseAndSetIfChanged(ref _empRole, value); }
    private string _empPosition = string.Empty;
    public string EmpPosition { get => _empPosition; set => this.RaiseAndSetIfChanged(ref _empPosition, value); }
    private string _empDepartment = string.Empty;
    public string EmpDepartment { get => _empDepartment; set => this.RaiseAndSetIfChanged(ref _empDepartment, value); }
    private string _empPhone = string.Empty;
    public string EmpPhone { get => _empPhone; set => this.RaiseAndSetIfChanged(ref _empPhone, value); }
    private string _empEmail = string.Empty;
    public string EmpEmail { get => _empEmail; set => this.RaiseAndSetIfChanged(ref _empEmail, value); }
    private string _empNationalId = string.Empty;
    public string EmpNationalId { get => _empNationalId; set => this.RaiseAndSetIfChanged(ref _empNationalId, value); }
    private string _empAddress = string.Empty;
    public string EmpAddress { get => _empAddress; set => this.RaiseAndSetIfChanged(ref _empAddress, value); }
    private string _empEmergencyContact = string.Empty;
    public string EmpEmergencyContact { get => _empEmergencyContact; set => this.RaiseAndSetIfChanged(ref _empEmergencyContact, value); }
    private string _empEmergencyPhone = string.Empty;
    public string EmpEmergencyPhone { get => _empEmergencyPhone; set => this.RaiseAndSetIfChanged(ref _empEmergencyPhone, value); }
    private decimal _empBaseSalary;
    public decimal EmpBaseSalary { get => _empBaseSalary; set => this.RaiseAndSetIfChanged(ref _empBaseSalary, value); }
    private DateTime _empJoinDate = DateTime.Now;
    public DateTime EmpJoinDate { get => _empJoinDate; set => this.RaiseAndSetIfChanged(ref _empJoinDate, value); }
    private string _empNotes = string.Empty;
    public string EmpNotes { get => _empNotes; set => this.RaiseAndSetIfChanged(ref _empNotes, value); }
    private string _empPhotoPath = string.Empty;
    public string EmpPhotoPath { get => _empPhotoPath; set => this.RaiseAndSetIfChanged(ref _empPhotoPath, value); }
    private string _empContractPath = string.Empty;
    public string EmpContractPath { get => _empContractPath; set => this.RaiseAndSetIfChanged(ref _empContractPath, value); }
    private string _empCvPath = string.Empty;
    public string EmpCvPath { get => _empCvPath; set => this.RaiseAndSetIfChanged(ref _empCvPath, value); }

    // ─── Payroll Form ─────────────────────────────────────────────
    private bool _isPayrollFormOpen;
    public bool IsPayrollFormOpen
    {
        get => _isPayrollFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isPayrollFormOpen, value);
    }

    private int _payMonth = DateTime.Now.Month;
    public int PayMonth { get => _payMonth; set => this.RaiseAndSetIfChanged(ref _payMonth, value); }
    private int _payYear = DateTime.Now.Year;
    public int PayYear { get => _payYear; set => this.RaiseAndSetIfChanged(ref _payYear, value); }
    private decimal _payBaseSalary;
    public decimal PayBaseSalary { get => _payBaseSalary; set => this.RaiseAndSetIfChanged(ref _payBaseSalary, value); }
    private decimal _payHousingAllowance;
    public decimal PayHousingAllowance { get => _payHousingAllowance; set => this.RaiseAndSetIfChanged(ref _payHousingAllowance, value); }
    private decimal _payTransportAllowance;
    public decimal PayTransportAllowance { get => _payTransportAllowance; set => this.RaiseAndSetIfChanged(ref _payTransportAllowance, value); }
    private decimal _payOtherAllowances;
    public decimal PayOtherAllowances { get => _payOtherAllowances; set => this.RaiseAndSetIfChanged(ref _payOtherAllowances, value); }
    private decimal _payIncentives;
    public decimal PayIncentives { get => _payIncentives; set => this.RaiseAndSetIfChanged(ref _payIncentives, value); }
    private decimal _payDeductions;
    public decimal PayDeductions { get => _payDeductions; set => this.RaiseAndSetIfChanged(ref _payDeductions, value); }
    private int _payAbsenceDays;
    public int PayAbsenceDays { get => _payAbsenceDays; set => this.RaiseAndSetIfChanged(ref _payAbsenceDays, value); }
    private decimal _payAbsenceDeduction;
    public decimal PayAbsenceDeduction { get => _payAbsenceDeduction; set => this.RaiseAndSetIfChanged(ref _payAbsenceDeduction, value); }
    private string _payNotes = string.Empty;
    public string PayNotes { get => _payNotes; set => this.RaiseAndSetIfChanged(ref _payNotes, value); }

    public decimal PayNetSalary =>
        PayBaseSalary + PayHousingAllowance + PayTransportAllowance + PayOtherAllowances
        + PayIncentives - PayDeductions - PayAbsenceDeduction;

    // ─── Attendance ───────────────────────────────────────────────
    private int _attMonth = DateTime.Now.Month;
    public int AttMonth
    {
        get => _attMonth;
        set { this.RaiseAndSetIfChanged(ref _attMonth, value); _ = LoadAttendanceAsync(); }
    }

    private int _attYear = DateTime.Now.Year;
    public int AttYear
    {
        get => _attYear;
        set { this.RaiseAndSetIfChanged(ref _attYear, value); _ = LoadAttendanceAsync(); }
    }

    public int TotalPresent => AttendanceRecords.Count(a => a.Status == "Present");
    public int TotalAbsent => AttendanceRecords.Count(a => a.Status == "Absent");
    public int TotalLate => AttendanceRecords.Count(a => a.Status == "Late");
    public int TotalOnLeave => AttendanceRecords.Count(a => a.Status == "OnLeave");

    // ─── Leave Form ───────────────────────────────────────────────
    private bool _isLeaveFormOpen;
    public bool IsLeaveFormOpen
    {
        get => _isLeaveFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isLeaveFormOpen, value);
    }

    private string _leaveType = "Annual";
    public string LeaveType { get => _leaveType; set => this.RaiseAndSetIfChanged(ref _leaveType, value); }
    private DateTime _leaveStart = DateTime.Now;
    public DateTime LeaveStart { get => _leaveStart; set => this.RaiseAndSetIfChanged(ref _leaveStart, value); }
    private DateTime _leaveEnd = DateTime.Now.AddDays(1);
    public DateTime LeaveEnd { get => _leaveEnd; set => this.RaiseAndSetIfChanged(ref _leaveEnd, value); }
    private string _leaveReason = string.Empty;
    public string LeaveReason { get => _leaveReason; set => this.RaiseAndSetIfChanged(ref _leaveReason, value); }

    // ─── ID Card ──────────────────────────────────────────────────
    private bool _isIdCardOpen;
    public bool IsIdCardOpen
    {
        get => _isIdCardOpen;
        set => this.RaiseAndSetIfChanged(ref _isIdCardOpen, value);
    }

    // ─── Months list ─────────────────────────────────────────────
    public List<string> MonthNames { get; } = new List<string>
    {
        "يناير","فبراير","مارس","أبريل","مايو","يونيو",
        "يوليو","أغسطس","سبتمبر","أكتوبر","نوفمبر","ديسمبر"
    };

    public List<string> RoleOptions { get; } = new List<string>
    { "Therapist", "Receptionist", "Admin", "Accountant", "Doctor", "Nurse", "Driver", "Other" };

    public List<string> DepartmentOptions { get; } = new List<string>
    { "العيادة", "الإدارة", "المالية", "التأهيل", "التعليم الخاص", "العلاج الطبيعي", "أخرى" };

    // ─── Commands ─────────────────────────────────────────────────
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddEmployeeCommand { get; }
    public ReactiveCommand<Unit, Unit> EditEmployeeCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveEmployeeCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowIdCardCommand { get; }
    public ReactiveCommand<Unit, Unit> PrintIdCardCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenContractCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenCvCommand { get; }
    public ReactiveCommand<Unit, Unit> AddPayrollCommand { get; }
    public ReactiveCommand<Unit, Unit> SavePayrollCommand { get; }
    public ReactiveCommand<Unit, Unit> MarkPayrollPaidCommand { get; }
    public ReactiveCommand<Unit, Unit> PrintPayrollCommand { get; }
    public ReactiveCommand<Unit, Unit> AddAttendanceCommand { get; }
    public ReactiveCommand<Unit, Unit> RequestLeaveCommand { get; }
    public ReactiveCommand<Unit, Unit> SubmitLeaveCommand { get; }
    public ReactiveCommand<Unit, Unit> ApproveLeaveCommand { get; }
    public ReactiveCommand<Unit, Unit> RejectLeaveCommand { get; }
    public ReactiveCommand<Unit, Unit> EvaluateTherapistCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public HRManagementViewModel(DatabaseService dbService, DialogService dialogService)
    {
        _dbService = dbService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddEmployeeCommand = ReactiveCommand.Create(OpenAddEmployeeForm);
        EditEmployeeCommand = ReactiveCommand.Create(OpenEditEmployeeForm);
        SaveEmployeeCommand = ReactiveCommand.CreateFromTask(SaveEmployeeAsync);
        ShowIdCardCommand = ReactiveCommand.Create(() => { if (SelectedEmployee != null) IsIdCardOpen = true; });
        PrintIdCardCommand = ReactiveCommand.CreateFromTask(PrintIdCardAsync);
        OpenContractCommand = ReactiveCommand.Create(OpenContract);
        OpenCvCommand = ReactiveCommand.Create(OpenCv);

        AddPayrollCommand = ReactiveCommand.Create(OpenAddPayrollForm);
        SavePayrollCommand = ReactiveCommand.CreateFromTask(SavePayrollAsync);
        MarkPayrollPaidCommand = ReactiveCommand.CreateFromTask(MarkPayrollPaidAsync);
        PrintPayrollCommand = ReactiveCommand.CreateFromTask(PrintPayrollAsync);

        AddAttendanceCommand = ReactiveCommand.CreateFromTask(AddAttendanceAsync);

        RequestLeaveCommand = ReactiveCommand.Create(() => { IsLeaveFormOpen = true; });
        SubmitLeaveCommand = ReactiveCommand.CreateFromTask(SubmitLeaveAsync);
        ApproveLeaveCommand = ReactiveCommand.CreateFromTask(ApproveLeaveAsync);
        RejectLeaveCommand = ReactiveCommand.CreateFromTask(RejectLeaveAsync);
        EvaluateTherapistCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedEmployee != null)
                await _dialogService.ShowInfoAsync("تقييم الموظف", $"سيتم فتح نموذج تقييم {SelectedEmployee.Name}");
        });
        CancelCommand = ReactiveCommand.Create(() =>
        {
            IsLeaveFormOpen = false;
            IsEmployeeFormOpen = false;
            IsPayrollFormOpen = false;
            IsIdCardOpen = false;
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        _allEmployees = await _dbService.GetEmployeesAsync();
        FilterEmployees();

        var leaveList = await _dbService.GetLeaveRequestsAsync();
        LeaveRequests.Clear();
        foreach (var l in leaveList) LeaveRequests.Add(l);

        if (SelectedEmployee != null)
            await LoadEmployeeDetailsAsync(SelectedEmployee);
    }

    private void FilterEmployees()
    {
        Employees.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allEmployees
            : _allEmployees.Where(e =>
                e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (e.EmployeeNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (e.Phone?.Contains(SearchText) ?? false) ||
                (e.Position?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (e.Department?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (e.NationalId?.Contains(SearchText) ?? false));
        foreach (var e in filtered) Employees.Add(e);
    }

    private async Task LoadEmployeeDetailsAsync(Employee emp)
    {
        var payrolls = await _dbService.GetPayrollsAsync(employeeId: emp.Id);
        Payrolls.Clear();
        foreach (var p in payrolls) Payrolls.Add(p);

        await LoadAttendanceAsync();
    }

    private async Task LoadAttendanceAsync()
    {
        if (SelectedEmployee == null) return;
        var records = await _dbService.GetMonthlyAttendanceAsync(SelectedEmployee.Id, AttMonth, AttYear);
        AttendanceRecords.Clear();
        foreach (var r in records) AttendanceRecords.Add(r);
        this.RaisePropertyChanged(nameof(TotalPresent));
        this.RaisePropertyChanged(nameof(TotalAbsent));
        this.RaisePropertyChanged(nameof(TotalLate));
        this.RaisePropertyChanged(nameof(TotalOnLeave));
    }

    // ─── Employee Form ────────────────────────────────────────────
    private void OpenAddEmployeeForm()
    {
        IsEditMode = false;
        FormTitle = "إضافة موظف جديد";
        EmpNumber = $"EMP-{DateTime.Now:yyyyMMddHHmm}";
        EmpName = EmpRole = EmpPosition = EmpDepartment = EmpPhone = EmpEmail =
            EmpNationalId = EmpAddress = EmpEmergencyContact = EmpEmergencyPhone =
            EmpNotes = EmpPhotoPath = EmpContractPath = EmpCvPath = string.Empty;
        EmpRole = "Therapist";
        EmpBaseSalary = 0;
        EmpJoinDate = DateTime.Now;
        IsEmployeeFormOpen = true;
    }

    private void OpenEditEmployeeForm()
    {
        if (SelectedEmployee == null) return;
        IsEditMode = true;
        FormTitle = "تعديل بيانات الموظف";
        var e = SelectedEmployee;
        EmpNumber = e.EmployeeNumber;
        EmpName = e.Name;
        EmpRole = e.Role;
        EmpPosition = e.Position ?? "";
        EmpDepartment = e.Department ?? "";
        EmpPhone = e.Phone ?? "";
        EmpEmail = e.Email ?? "";
        EmpNationalId = e.NationalId ?? "";
        EmpAddress = e.Address ?? "";
        EmpEmergencyContact = e.EmergencyContact ?? "";
        EmpEmergencyPhone = e.EmergencyPhone ?? "";
        EmpBaseSalary = e.BaseSalary;
        EmpJoinDate = e.JoinDate;
        EmpNotes = e.Notes ?? "";
        EmpPhotoPath = e.PhotoPath ?? "";
        EmpContractPath = e.ContractPath ?? "";
        EmpCvPath = e.CvPath ?? "";
        IsEmployeeFormOpen = true;
    }

    private async Task SaveEmployeeAsync()
    {
        if (string.IsNullOrWhiteSpace(EmpName))
        {
            await _dialogService.ShowInfoAsync("خطأ", "اسم الموظف مطلوب");
            return;
        }

        if (IsEditMode && SelectedEmployee != null)
        {
            var e = SelectedEmployee;
            e.EmployeeNumber = EmpNumber;
            e.Name = EmpName;
            e.Role = EmpRole;
            e.Position = EmpPosition;
            e.Department = EmpDepartment;
            e.Phone = EmpPhone;
            e.Email = EmpEmail;
            e.NationalId = EmpNationalId;
            e.Address = EmpAddress;
            e.EmergencyContact = EmpEmergencyContact;
            e.EmergencyPhone = EmpEmergencyPhone;
            e.BaseSalary = EmpBaseSalary;
            e.Salary = EmpBaseSalary;
            e.JoinDate = EmpJoinDate;
            e.Notes = EmpNotes;
            e.PhotoPath = EmpPhotoPath;
            e.ContractPath = EmpContractPath;
            e.CvPath = EmpCvPath;
            await _dbService.UpdateEmployeeAsync(e);
            await AuditLogger.LogAsync("Update", "Employee", $"تعديل بيانات {e.Name}");
        }
        else
        {
            var emp = new Employee
            {
                EmployeeNumber = EmpNumber,
                Name = EmpName,
                Role = EmpRole,
                Position = EmpPosition,
                Department = EmpDepartment,
                Phone = EmpPhone,
                Email = EmpEmail,
                NationalId = EmpNationalId,
                Address = EmpAddress,
                EmergencyContact = EmpEmergencyContact,
                EmergencyPhone = EmpEmergencyPhone,
                BaseSalary = EmpBaseSalary,
                Salary = EmpBaseSalary,
                JoinDate = EmpJoinDate,
                Notes = EmpNotes,
                PhotoPath = EmpPhotoPath,
                ContractPath = EmpContractPath,
                CvPath = EmpCvPath
            };
            await _dbService.AddEmployeeAsync(emp);
            await AuditLogger.LogAsync("Create", "Employee", $"إضافة موظف {emp.Name}");
        }

        IsEmployeeFormOpen = false;
        await LoadAsync();
    }

    // ─── ID Card ──────────────────────────────────────────────────
    private async Task PrintIdCardAsync()
    {
        if (SelectedEmployee == null) return;
        await _dialogService.ShowInfoAsync("طباعة البطاقة", $"جاري طباعة بطاقة {SelectedEmployee.Name}...");
    }

    private void OpenContract()
    {
        if (SelectedEmployee == null || string.IsNullOrEmpty(SelectedEmployee.ContractPath)) return;
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(SelectedEmployee.ContractPath) { UseShellExecute = true }); }
        catch { }
    }

    private void OpenCv()
    {
        if (SelectedEmployee == null || string.IsNullOrEmpty(SelectedEmployee.CvPath)) return;
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(SelectedEmployee.CvPath) { UseShellExecute = true }); }
        catch { }
    }

    // ─── Payroll ──────────────────────────────────────────────────
    private void OpenAddPayrollForm()
    {
        if (SelectedEmployee == null) return;
        PayMonth = DateTime.Now.Month;
        PayYear = DateTime.Now.Year;
        PayBaseSalary = SelectedEmployee.BaseSalary;
        PayHousingAllowance = PayTransportAllowance = PayOtherAllowances =
            PayIncentives = PayDeductions = PayAbsenceDeduction = 0;
        PayAbsenceDays = 0;
        PayNotes = "";
        IsPayrollFormOpen = true;
    }

    private async Task SavePayrollAsync()
    {
        if (SelectedEmployee == null) return;
        var payroll = new EmployeePayroll
        {
            EmployeeId = SelectedEmployee.Id,
            Month = PayMonth,
            Year = PayYear,
            BaseSalary = PayBaseSalary,
            HousingAllowance = PayHousingAllowance,
            TransportAllowance = PayTransportAllowance,
            OtherAllowances = PayOtherAllowances,
            Incentives = PayIncentives,
            Deductions = PayDeductions,
            AbsenceDays = PayAbsenceDays,
            AbsenceDeduction = PayAbsenceDeduction,
            Notes = PayNotes,
            Status = "Pending"
        };
        await _dbService.AddPayrollAsync(payroll);
        await AuditLogger.LogAsync("Create", "Payroll", $"راتب {SelectedEmployee.Name} شهر {PayMonth}/{PayYear}");
        IsPayrollFormOpen = false;
        await LoadEmployeeDetailsAsync(SelectedEmployee);
    }

    private async Task MarkPayrollPaidAsync()
    {
        if (SelectedPayroll == null) return;
        SelectedPayroll.Status = "Paid";
        SelectedPayroll.PaidDate = DateTime.Now;
        await _dbService.UpdatePayrollAsync(SelectedPayroll);
        if (SelectedEmployee != null)
            await LoadEmployeeDetailsAsync(SelectedEmployee);
    }

    private async Task PrintPayrollAsync()
    {
        if (SelectedEmployee == null) return;
        await _dialogService.ShowInfoAsync("كشف رواتب", $"جاري طباعة كشف رواتب {SelectedEmployee.Name}");
    }

    // ─── Attendance ───────────────────────────────────────────────
    private async Task AddAttendanceAsync()
    {
        if (SelectedEmployee == null) return;
        var att = new EmployeeAttendance
        {
            EmployeeId = SelectedEmployee.Id,
            Date = DateTime.Now,
            Status = "Present"
        };
        await _dbService.AddEmployeeAttendanceAsync(att);
        await LoadAttendanceAsync();
    }

    // ─── Leave ────────────────────────────────────────────────────
    private async Task SubmitLeaveAsync()
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
        LeaveReason = string.Empty;
        await LoadAsync();
        await AuditLogger.LogAsync("Create", "LeaveRequest", $"{SelectedEmployee.Name} طلب إجازة {LeaveType}");
    }

    private async Task ApproveLeaveAsync()
    {
        if (SelectedEmployee == null) return;
        var leave = LeaveRequests.FirstOrDefault(l => l.EmployeeId == SelectedEmployee.Id && l.Status == "Pending");
        if (leave != null)
        {
            leave.Status = "Approved";
            leave.ApprovalDate = DateTime.Now;
            await _dbService.UpdateLeaveRequestAsync(leave);
            await LoadAsync();
        }
    }

    private async Task RejectLeaveAsync()
    {
        if (SelectedEmployee == null) return;
        var leave = LeaveRequests.FirstOrDefault(l => l.EmployeeId == SelectedEmployee.Id && l.Status == "Pending");
        if (leave != null)
        {
            leave.Status = "Rejected";
            await _dbService.UpdateLeaveRequestAsync(leave);
            await LoadAsync();
        }
    }
}
