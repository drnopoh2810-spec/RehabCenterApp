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

public class AccountingViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly PrintService _printService;
    private readonly DialogService _dialogService;

    private ObservableCollection<Payment> _payments = new();
    public ObservableCollection<Payment> Payments
    {
        get => _payments;
        set => this.RaiseAndSetIfChanged(ref _payments, value);
    }

    private ObservableCollection<Expense> _expenses = new();
    public ObservableCollection<Expense> Expenses
    {
        get => _expenses;
        set => this.RaiseAndSetIfChanged(ref _expenses, value);
    }

    private DateTime _startDate = DateTime.Now.AddMonths(-1);
    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _startDate, value);
            _ = LoadDataAsync();
        }
    }

    private DateTime _endDate = DateTime.Now;
    public DateTime EndDate
    {
        get => _endDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _endDate, value);
            _ = LoadDataAsync();
        }
    }

    private decimal _totalRevenue;
    public decimal TotalRevenue
    {
        get => _totalRevenue;
        set => this.RaiseAndSetIfChanged(ref _totalRevenue, value);
    }

    private decimal _totalExpenses;
    public decimal TotalExpenses
    {
        get => _totalExpenses;
        set => this.RaiseAndSetIfChanged(ref _totalExpenses, value);
    }

    private decimal _netProfit;
    public decimal NetProfit
    {
        get => _netProfit;
        set => this.RaiseAndSetIfChanged(ref _netProfit, value);
    }

    private int _selectedTab = 0;
    public int SelectedTab
    {
        get => _selectedTab;
        set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
    }

    private Payment? _selectedPayment;
    public Payment? SelectedPayment
    {
        get => _selectedPayment;
        set => this.RaiseAndSetIfChanged(ref _selectedPayment, value);
    }

    // Payment Form
    private bool _isPaymentFormOpen;
    public bool IsPaymentFormOpen
    {
        get => _isPaymentFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaymentFormOpen, value);
    }

    private Beneficiary? _selectedBeneficiary;
    public Beneficiary? SelectedBeneficiary
    {
        get => _selectedBeneficiary;
        set => this.RaiseAndSetIfChanged(ref _selectedBeneficiary, value);
    }

    private decimal _paymentAmount;
    public decimal PaymentAmount
    {
        get => _paymentAmount;
        set => this.RaiseAndSetIfChanged(ref _paymentAmount, value);
    }

    private string _paymentType = "نقدي";
    public string PaymentType
    {
        get => _paymentType;
        set => this.RaiseAndSetIfChanged(ref _paymentType, value);
    }

    private string _paymentNotes = string.Empty;
    public string PaymentNotes
    {
        get => _paymentNotes;
        set => this.RaiseAndSetIfChanged(ref _paymentNotes, value);
    }

    // Expense Form
    private bool _isExpenseFormOpen;
    public bool IsExpenseFormOpen
    {
        get => _isExpenseFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isExpenseFormOpen, value);
    }

    private string _expenseCategory = string.Empty;
    public string ExpenseCategory
    {
        get => _expenseCategory;
        set => this.RaiseAndSetIfChanged(ref _expenseCategory, value);
    }

    private decimal _expenseAmount;
    public decimal ExpenseAmount
    {
        get => _expenseAmount;
        set => this.RaiseAndSetIfChanged(ref _expenseAmount, value);
    }

    private string _expenseDescription = string.Empty;
    public string ExpenseDescription
    {
        get => _expenseDescription;
        set => this.RaiseAndSetIfChanged(ref _expenseDescription, value);
    }

    public ObservableCollection<Beneficiary> Beneficiaries { get; } = new();

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddPaymentCommand { get; }
    public ReactiveCommand<Unit, Unit> SavePaymentCommand { get; }
    public ReactiveCommand<Unit, Unit> AddExpenseCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveExpenseCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> PrintReceiptCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportPaymentsCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportExpensesCommand { get; }

    public AccountingViewModel(DatabaseService dbService, PrintService printService, DialogService dialogService)
    {
        _dbService = dbService;
        _printService = printService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);
        AddPaymentCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var list = await _dbService.GetBeneficiariesAsync();
            Beneficiaries.Clear();
            foreach (var b in list) Beneficiaries.Add(b);
            IsPaymentFormOpen = true;
        });
        SavePaymentCommand = ReactiveCommand.CreateFromTask(SavePaymentAsync);
        AddExpenseCommand = ReactiveCommand.Create(() => { IsExpenseFormOpen = true; });
        SaveExpenseCommand = ReactiveCommand.CreateFromTask(SaveExpenseAsync);
        CancelCommand = ReactiveCommand.Create(() =>
        {
            IsPaymentFormOpen = false;
            IsExpenseFormOpen = false;
        });
        PrintReceiptCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedPayment != null)
            {
                var centerName = await _dbService.GetSettingAsync("CenterName") ?? "مركز التأهيل";
                var centerPhone = await _dbService.GetSettingAsync("CenterPhone") ?? "";
                var path = await _printService.GenerateReceiptAsync(SelectedPayment, centerName, centerPhone);
                await _dialogService.ShowInfoAsync("تم الطباعة", $"تم حفظ الإيصال: {path}");
                await AuditLogger.LogAsync("Print", "Receipt", $"Printed receipt: {SelectedPayment.ReceiptNumber}");
            }
            else
            {
                await _dialogService.ShowErrorAsync("خطأ", "الرجاء اختيار دفعة أولاً");
            }
        });
        ExportPaymentsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var list = Payments.ToList();
            if (list.Count > 0)
            {
                var path = await ExcelExporter.ExportPaymentsAsync(list);
                await _dialogService.ShowInfoAsync("تم التصدير", $"تم حفظ الملف: {path}");
            }
        });
        ExportExpensesCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var list = Expenses.ToList();
            if (list.Count > 0)
            {
                var path = await ExcelExporter.ExportExpensesAsync(list);
                await _dialogService.ShowInfoAsync("تم التصدير", $"تم حفظ الملف: {path}");
            }
        });

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var payments = await _dbService.GetPaymentsAsync(StartDate, EndDate);
        Payments.Clear();
        foreach (var p in payments) Payments.Add(p);

        var expenses = await _dbService.GetExpensesAsync(StartDate, EndDate);
        Expenses.Clear();
        foreach (var e in expenses) Expenses.Add(e);

        TotalRevenue = await _dbService.GetTotalRevenueAsync(StartDate, EndDate);
        TotalExpenses = await _dbService.GetTotalExpensesAsync(StartDate, EndDate);
        NetProfit = TotalRevenue - TotalExpenses;
    }

    private async Task SavePaymentAsync()
    {
        if (SelectedBeneficiary == null || PaymentAmount <= 0)
            return;

        var payment = new Payment
        {
            BeneficiaryId = SelectedBeneficiary.Id,
            Amount = PaymentAmount,
            Date = DateTime.Now,
            PaymentType = PaymentType,
            ReceiptNumber = $"REC-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            Notes = PaymentNotes
        };

        await _dbService.AddPaymentAsync(payment);
        IsPaymentFormOpen = false;
        PaymentAmount = 0;
        PaymentNotes = string.Empty;
        await LoadDataAsync();
        await AuditLogger.LogAsync("Create", "Payment", $"Added payment: {payment.ReceiptNumber} for {SelectedBeneficiary.Name}");
    }

    private async Task SaveExpenseAsync()
    {
        if (string.IsNullOrWhiteSpace(ExpenseCategory) || ExpenseAmount <= 0)
            return;

        var expense = new Expense
        {
            Category = ExpenseCategory,
            Amount = ExpenseAmount,
            Date = DateTime.Now,
            Description = ExpenseDescription
        };

        await _dbService.AddExpenseAsync(expense);
        IsExpenseFormOpen = false;
        ExpenseCategory = string.Empty;
        ExpenseAmount = 0;
        ExpenseDescription = string.Empty;
        await LoadDataAsync();
        await AuditLogger.LogAsync("Create", "Expense", $"Added expense: {expense.Category} - {expense.Amount}");
    }
}