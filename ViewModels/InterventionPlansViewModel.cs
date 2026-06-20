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

public class InterventionPlansViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    private ObservableCollection<InterventionPlan> _plans = new();
    public ObservableCollection<InterventionPlan> Plans
    {
        get => _plans;
        set => this.RaiseAndSetIfChanged(ref _plans, value);
    }

    private InterventionPlan? _selectedPlan;
    public InterventionPlan? SelectedPlan
    {
        get => _selectedPlan;
        set => this.RaiseAndSetIfChanged(ref _selectedPlan, value);
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

    private string _planTitle = string.Empty;
    public string PlanTitle
    {
        get => _planTitle;
        set => this.RaiseAndSetIfChanged(ref _planTitle, value);
    }

    private string _longTermGoals = string.Empty;
    public string LongTermGoals
    {
        get => _longTermGoals;
        set => this.RaiseAndSetIfChanged(ref _longTermGoals, value);
    }

    public ObservableCollection<Beneficiary> Beneficiaries { get; } = new();
    public ObservableCollection<PlanObjective> Objectives { get; } = new();

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> AddObjectiveCommand { get; }
    public ReactiveCommand<Unit, Unit> PrintIEPCommand { get; }

    public InterventionPlansViewModel(DatabaseService dbService, DialogService dialogService)
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
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(() => { IsFormOpen = false; });
        AddObjectiveCommand = ReactiveCommand.Create(() => { });
        PrintIEPCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedPlan != null)
                await _dialogService.ShowInfoAsync("طباعة", "تم طباعة خطة التدخل");
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _dbService.GetInterventionPlansAsync();
        Plans.Clear();
        foreach (var p in list) Plans.Add(p);
    }

    private async Task SaveAsync()
    {
        if (SelectedBeneficiary == null || string.IsNullOrWhiteSpace(PlanTitle))
            return;

        var plan = new InterventionPlan
        {
            BeneficiaryId = SelectedBeneficiary.Id,
            PlanTitle = PlanTitle,
            LongTermGoals = LongTermGoals,
            StartDate = DateTime.Now,
            Status = "Active"
        };

        await _dbService.AddInterventionPlanAsync(plan);
        IsFormOpen = false;
        PlanTitle = string.Empty;
        LongTermGoals = string.Empty;
        await LoadAsync();
        await AuditLogger.LogAsync("Create", "InterventionPlan", $"Added IEP for {SelectedBeneficiary.Name}");
    }
}