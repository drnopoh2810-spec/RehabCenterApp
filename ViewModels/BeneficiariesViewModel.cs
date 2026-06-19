using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class BeneficiariesViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;
    private readonly PrintService _printService;

    private ObservableCollection<Beneficiary> _beneficiaries = new();
    public ObservableCollection<Beneficiary> Beneficiaries
    {
        get => _beneficiaries;
        set => this.RaiseAndSetIfChanged(ref _beneficiaries, value);
    }

    private Beneficiary? _selectedBeneficiary;
    public Beneficiary? SelectedBeneficiary
    {
        get => _selectedBeneficiary;
        set => this.RaiseAndSetIfChanged(ref _selectedBeneficiary, value);
    }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchQuery, value);
            _ = SearchAsync();
        }
    }

    private bool _isFormOpen;
    public bool IsFormOpen
    {
        get => _isFormOpen;
        set => this.RaiseAndSetIfChanged(ref _isFormOpen, value);
    }

    private BeneficiaryFormViewModel _formViewModel = null!;
    public BeneficiaryFormViewModel FormViewModel
    {
        get => _formViewModel;
        set => this.RaiseAndSetIfChanged(ref _formViewModel, value);
    }

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseFormCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportToPdfCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportToExcelCommand { get; }

    public BeneficiariesViewModel(DatabaseService dbService, DialogService dialogService, PrintService printService)
    {
        _dbService = dbService;
        _dialogService = dialogService;
        _printService = printService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AddCommand = ReactiveCommand.Create(() =>
        {
            FormViewModel = new BeneficiaryFormViewModel(_dbService, null, OnFormSaved);
            IsFormOpen = true;
        });
        EditCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedBeneficiary != null)
            {
                FormViewModel = new BeneficiaryFormViewModel(_dbService, SelectedBeneficiary, OnFormSaved);
                IsFormOpen = true;
            }
        });
        DeleteCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedBeneficiary != null)
            {
                var confirm = await _dialogService.ShowConfirmAsync("تأكيد الحذف",
                    $"هل أنت متأكد من حذف {SelectedBeneficiary.Name}؟");
                if (confirm)
                {
                    await _dbService.DeleteBeneficiaryAsync(SelectedBeneficiary.Id);
                    await LoadAsync();
                    await AuditLogger.LogAsync("Delete", "Beneficiary", $"Deleted beneficiary: {SelectedBeneficiary.Name}");
                }
            }
        });
        CloseFormCommand = ReactiveCommand.Create(() => IsFormOpen = false);
        ExportToPdfCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var list = Beneficiaries.ToList();
            if (list.Count > 0)
            {
                var path = await _printService.ExportBeneficiariesToPdfAsync(list);
                await _dialogService.ShowInfoAsync("تم التصدير", $"تم حفظ الملف: {path}");
            }
        });
        ExportToExcelCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var list = Beneficiaries.ToList();
            if (list.Count > 0)
            {
                var path = await ExcelExporter.ExportBeneficiariesToExcelAsync(list);
                await _dialogService.ShowInfoAsync("تم التصدير", $"تم حفظ الملف: {path}");
            }
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _dbService.GetBeneficiariesAsync();
        Beneficiaries.Clear();
        foreach (var b in list)
            Beneficiaries.Add(b);
    }

    private async Task SearchAsync()
    {
        var list = await _dbService.GetBeneficiariesAsync(SearchQuery);
        Beneficiaries.Clear();
        foreach (var b in list)
            Beneficiaries.Add(b);
    }

    private async void OnFormSaved()
    {
        IsFormOpen = false;
        await LoadAsync();
    }
}