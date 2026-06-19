using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.ViewModels;

public class GamificationViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;

    private ObservableCollection<Achievement> _achievements = new();
    public ObservableCollection<Achievement> Achievements
    {
        get => _achievements;
        set => this.RaiseAndSetIfChanged(ref _achievements, value);
    }

    private Beneficiary? _selectedBeneficiary;
    public Beneficiary? SelectedBeneficiary
    {
        get => _selectedBeneficiary;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedBeneficiary, value);
            _ = LoadBeneficiaryAchievementsAsync();
        }
    }

    private int _totalPoints;
    public int TotalPoints
    {
        get => _totalPoints;
        set => this.RaiseAndSetIfChanged(ref _totalPoints, value);
    }

    private int _currentLevel;
    public int CurrentLevel
    {
        get => _currentLevel;
        set => this.RaiseAndSetIfChanged(ref _currentLevel, value);
    }

    private string _levelTitle = "مبتدئ";
    public string LevelTitle
    {
        get => _levelTitle;
        set => this.RaiseAndSetIfChanged(ref _levelTitle, value);
    }

    public ObservableCollection<Beneficiary> Beneficiaries { get; } = new();

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AwardAchievementCommand { get; }
    public ReactiveCommand<Unit, Unit> PrintCertificateCommand { get; }

    public GamificationViewModel(DatabaseService dbService, DialogService dialogService)
    {
        _dbService = dbService;
        _dialogService = dialogService;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        AwardAchievementCommand = ReactiveCommand.CreateFromTask(AwardAsync);
        PrintCertificateCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedBeneficiary != null)
                await _dialogService.ShowInfoAsync("شهادة", $"تم طباعة شهادة إنجاز لـ {SelectedBeneficiary.Name}");
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _dbService.GetBeneficiariesAsync();
        Beneficiaries.Clear();
        foreach (var b in list) Beneficiaries.Add(b);
    }

    private async Task LoadBeneficiaryAchievementsAsync()
    {
        if (SelectedBeneficiary == null) return;

        var achievements = await _dbService.GetAchievementsAsync(SelectedBeneficiary.Id);
        Achievements.Clear();
        foreach (var a in achievements) Achievements.Add(a);

        TotalPoints = achievements.Sum(a => a.Points);
        CurrentLevel = CalculateLevel(TotalPoints);
        LevelTitle = GetLevelTitle(CurrentLevel);
    }

    private async Task AwardAsync()
    {
        if (SelectedBeneficiary == null) return;

        var achievement = new Achievement
        {
            BeneficiaryId = SelectedBeneficiary.Id,
            Title = "إنجاز جديد",
            Category = "General",
            Points = 10,
            AchievementDate = DateTime.Now,
            Description = "تم منحه تلقائياً من النظام"
        };

        await _dbService.AddAchievementAsync(achievement);
        await LoadBeneficiaryAchievementsAsync();
        await _dialogService.ShowInfoAsync("مبروك!", $"تم منح {SelectedBeneficiary.Name} 10 نقاط جديدة!");
    }

    private static int CalculateLevel(int points) => points switch
    {
        < 50 => 1,
        < 100 => 2,
        < 200 => 3,
        < 350 => 4,
        < 500 => 5,
        _ => 6
    };

    private static string GetLevelTitle(int level) => level switch
    {
        1 => "مبتدئ",
        2 => "متعلم",
        3 => "ممارس",
        4 => "متقدم",
        5 => "خبير",
        _ => "بطل"
    };
}