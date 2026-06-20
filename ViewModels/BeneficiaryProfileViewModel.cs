using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RehabCenterApp.Services;
using RehabCenterApp.Models;

namespace RehabCenterApp.ViewModels;

/// <summary>
/// A single entry in the beneficiary's history timeline.
/// Composed from sessions, therapist reports, assessments, and plan progress.
/// </summary>
public class HistoryEntry
{
    public DateTime Date { get; set; }
    public string EntryType { get; set; } = string.Empty;   // جلسة | تقييم | خطة | تقدم
    public string TypeIcon { get; set; } = "CalendarCheck";
    public string TypeColor { get; set; } = "#4299e1";
    public string TherapistName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public decimal? MasteryPercent { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DateFormatted => Date.ToString("yyyy-MM-dd   HH:mm");
}

public class BeneficiaryProfileViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly DialogService _dialogService;
    private readonly Action _onClose;

    // ── Beneficiary data ──────────────────────────────────────────
    private Beneficiary _beneficiary = new();
    public Beneficiary Beneficiary
    {
        get => _beneficiary;
        set => this.RaiseAndSetIfChanged(ref _beneficiary, value);
    }

    // ── Tab index ─────────────────────────────────────────────────
    private int _tabIndex;
    public int TabIndex
    {
        get => _tabIndex;
        set => this.RaiseAndSetIfChanged(ref _tabIndex, value);
    }

    // ── Photo ─────────────────────────────────────────────────────
    private string? _photoPath;
    public string? PhotoPath
    {
        get => _photoPath;
        set => this.RaiseAndSetIfChanged(ref _photoPath, value);
    }

    public bool HasPhoto => !string.IsNullOrEmpty(PhotoPath) && File.Exists(PhotoPath);

    // ── Collections ───────────────────────────────────────────────
    public ObservableCollection<BeneficiaryAttachment> Attachments { get; } = new();
    public ObservableCollection<HistoryEntry> History { get; } = new();
    public ObservableCollection<Assessment> Assessments { get; } = new();
    public ObservableCollection<InterventionPlan> Plans { get; } = new();

    // ── Stats ─────────────────────────────────────────────────────
    private int _totalSessions;
    public int TotalSessions { get => _totalSessions; set => this.RaiseAndSetIfChanged(ref _totalSessions, value); }

    private decimal _avgMastery;
    public decimal AvgMastery { get => _avgMastery; set => this.RaiseAndSetIfChanged(ref _avgMastery, value); }

    private int _totalAssessments;
    public int TotalAssessments { get => _totalAssessments; set => this.RaiseAndSetIfChanged(ref _totalAssessments, value); }

    private int _activePlans;
    public int ActivePlans { get => _activePlans; set => this.RaiseAndSetIfChanged(ref _activePlans, value); }

    // ── Commands ──────────────────────────────────────────────────
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    public ReactiveCommand<Unit, Unit> PickPhotoCommand { get; }
    public ReactiveCommand<Unit, Unit> AddAttachmentCommand { get; }
    public ReactiveCommand<BeneficiaryAttachment, Unit> OpenAttachmentCommand { get; }
    public ReactiveCommand<BeneficiaryAttachment, Unit> DeleteAttachmentCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveBasicInfoCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveMedicalCommand { get; }

    // ── Editable fields - Basic ───────────────────────────────────
    private string _name = string.Empty;
    public string Name { get => _name; set => this.RaiseAndSetIfChanged(ref _name, value); }

    private DateTime _dateOfBirth = DateTime.Now.AddYears(-10);
    public DateTime DateOfBirth { get => _dateOfBirth; set => this.RaiseAndSetIfChanged(ref _dateOfBirth, value); }

    private string _gender = "ذكر";
    public string Gender { get => _gender; set => this.RaiseAndSetIfChanged(ref _gender, value); }

    private string _nationalId = string.Empty;
    public string NationalId { get => _nationalId; set => this.RaiseAndSetIfChanged(ref _nationalId, value); }

    private string _phone = string.Empty;
    public string Phone { get => _phone; set => this.RaiseAndSetIfChanged(ref _phone, value); }

    private string _address = string.Empty;
    public string Address { get => _address; set => this.RaiseAndSetIfChanged(ref _address, value); }

    private string _disabilityType = string.Empty;
    public string DisabilityType { get => _disabilityType; set => this.RaiseAndSetIfChanged(ref _disabilityType, value); }

    private string _guardianName = string.Empty;
    public string GuardianName { get => _guardianName; set => this.RaiseAndSetIfChanged(ref _guardianName, value); }

    private string _guardianPhone = string.Empty;
    public string GuardianPhone { get => _guardianPhone; set => this.RaiseAndSetIfChanged(ref _guardianPhone, value); }

    private string _guardianRelation = string.Empty;
    public string GuardianRelation { get => _guardianRelation; set => this.RaiseAndSetIfChanged(ref _guardianRelation, value); }

    private string _guardianEmail = string.Empty;
    public string GuardianEmail { get => _guardianEmail; set => this.RaiseAndSetIfChanged(ref _guardianEmail, value); }

    private string _emergencyPhone = string.Empty;
    public string EmergencyPhone { get => _emergencyPhone; set => this.RaiseAndSetIfChanged(ref _emergencyPhone, value); }

    private string _schoolName = string.Empty;
    public string SchoolName { get => _schoolName; set => this.RaiseAndSetIfChanged(ref _schoolName, value); }

    private string _referralSource = string.Empty;
    public string ReferralSource { get => _referralSource; set => this.RaiseAndSetIfChanged(ref _referralSource, value); }

    private string _status = "Active";
    public string Status { get => _status; set => this.RaiseAndSetIfChanged(ref _status, value); }

    // ── Editable fields - Medical ─────────────────────────────────
    private string _diagnosis = string.Empty;
    public string Diagnosis { get => _diagnosis; set => this.RaiseAndSetIfChanged(ref _diagnosis, value); }

    private string _secondaryDiagnosis = string.Empty;
    public string SecondaryDiagnosis { get => _secondaryDiagnosis; set => this.RaiseAndSetIfChanged(ref _secondaryDiagnosis, value); }

    private string _medicalHistory = string.Empty;
    public string MedicalHistory { get => _medicalHistory; set => this.RaiseAndSetIfChanged(ref _medicalHistory, value); }

    private string _allergies = string.Empty;
    public string Allergies { get => _allergies; set => this.RaiseAndSetIfChanged(ref _allergies, value); }

    private string _bloodType = string.Empty;
    public string BloodType { get => _bloodType; set => this.RaiseAndSetIfChanged(ref _bloodType, value); }

    private string _currentMedications = string.Empty;
    public string CurrentMedications { get => _currentMedications; set => this.RaiseAndSetIfChanged(ref _currentMedications, value); }

    private string _functionalLevel = string.Empty;
    public string FunctionalLevel { get => _functionalLevel; set => this.RaiseAndSetIfChanged(ref _functionalLevel, value); }

    private string _insuranceCompany = string.Empty;
    public string InsuranceCompany { get => _insuranceCompany; set => this.RaiseAndSetIfChanged(ref _insuranceCompany, value); }

    private string _insuranceNumber = string.Empty;
    public string InsuranceNumber { get => _insuranceNumber; set => this.RaiseAndSetIfChanged(ref _insuranceNumber, value); }

    // ─────────────────────────────────────────────────────────────
    public BeneficiaryProfileViewModel(
        DatabaseService dbService,
        DialogService dialogService,
        Beneficiary beneficiary,
        Action onClose)
    {
        _dbService = dbService;
        _dialogService = dialogService;
        _onClose = onClose;

        CloseCommand = ReactiveCommand.Create(() => _onClose());
        PickPhotoCommand = ReactiveCommand.CreateFromTask(PickPhotoAsync);
        AddAttachmentCommand = ReactiveCommand.CreateFromTask(AddAttachmentAsync);
        OpenAttachmentCommand = ReactiveCommand.Create<BeneficiaryAttachment>(OpenAttachment);
        DeleteAttachmentCommand = ReactiveCommand.CreateFromTask<BeneficiaryAttachment>(DeleteAttachmentAsync);
        SaveBasicInfoCommand = ReactiveCommand.CreateFromTask(SaveBasicInfoAsync);
        SaveMedicalCommand = ReactiveCommand.CreateFromTask(SaveMedicalAsync);

        LoadBeneficiary(beneficiary);
        _ = LoadAllDataAsync();
    }

    private void LoadBeneficiary(Beneficiary b)
    {
        Beneficiary = b;
        Name = b.Name;
        DateOfBirth = b.DateOfBirth == default ? DateTime.Now.AddYears(-10) : b.DateOfBirth;
        Gender = b.Gender;
        NationalId = b.NationalId ?? "";
        Phone = b.Phone ?? "";
        Address = b.Address ?? "";
        DisabilityType = b.DisabilityType;
        GuardianName = b.GuardianName ?? "";
        GuardianPhone = b.GuardianPhone ?? "";
        GuardianRelation = b.GuardianRelation ?? "";
        GuardianEmail = b.GuardianEmail ?? "";
        EmergencyPhone = b.EmergencyPhone ?? "";
        SchoolName = b.SchoolName ?? "";
        ReferralSource = b.ReferralSource ?? "";
        Status = b.Status;
        Diagnosis = b.Diagnosis ?? "";
        SecondaryDiagnosis = b.SecondaryDiagnosis ?? "";
        MedicalHistory = b.MedicalHistory ?? "";
        Allergies = b.Allergies ?? "";
        BloodType = b.BloodType ?? "";
        CurrentMedications = b.CurrentMedications ?? "";
        FunctionalLevel = b.FunctionalLevel ?? "";
        InsuranceCompany = b.InsuranceCompany ?? "";
        InsuranceNumber = b.InsuranceNumber ?? "";
        PhotoPath = b.PhotoPath;
    }

    private async Task LoadAllDataAsync()
    {
        if (Beneficiary.Id == 0) return;

        // Attachments
        var attachments = await _dbService.GetBeneficiaryAttachmentsAsync(Beneficiary.Id);
        Attachments.Clear();
        foreach (var a in attachments) Attachments.Add(a);

        // Assessments
        var assessments = await _dbService.GetBeneficiaryAssessmentsAsync(Beneficiary.Id);
        Assessments.Clear();
        foreach (var a in assessments) Assessments.Add(a);

        // Plans
        var plans = await _dbService.GetBeneficiaryPlansAsync(Beneficiary.Id);
        Plans.Clear();
        foreach (var p in plans) Plans.Add(p);

        // Therapist reports for history
        var reports = await _dbService.GetBeneficiaryTherapistReportsAsync(Beneficiary.Id);

        // Build history timeline
        var historyList = new List<HistoryEntry>();

        // From sessions
        if (Beneficiary.Sessions != null)
        {
            foreach (var s in Beneficiary.Sessions.Where(x => !x.IsDeleted))
            {
                historyList.Add(new HistoryEntry
                {
                    Date = s.Date,
                    EntryType = "جلسة",
                    TypeIcon = "CalendarCheck",
                    TypeColor = "#4299e1",
                    TherapistName = s.Therapist?.Name ?? "—",
                    Summary = $"جلسة {s.SessionType ?? ""} — {s.Status}",
                    MasteryPercent = null,
                    Status = s.Status
                });
            }
        }

        // From therapist reports
        foreach (var r in reports)
        {
            historyList.Add(new HistoryEntry
            {
                Date = r.ReportDate,
                EntryType = "تقرير يومي",
                TypeIcon = "FileDocument",
                TypeColor = "#48bb78",
                TherapistName = r.Therapist?.Name ?? "—",
                Summary = string.IsNullOrEmpty(r.ActivitiesPerformed)
                    ? "تقرير جلسة"
                    : r.ActivitiesPerformed.Length > 80
                        ? r.ActivitiesPerformed[..80] + "..."
                        : r.ActivitiesPerformed,
                MasteryPercent = r.OverallRating > 0 ? (decimal)(r.OverallRating * 20) : null,
                Status = r.Status
            });
        }

        // From assessments
        foreach (var a in assessments)
        {
            historyList.Add(new HistoryEntry
            {
                Date = a.AssessmentDate,
                EntryType = "تقييم",
                TypeIcon = "ChartBar",
                TypeColor = "#ed8936",
                TherapistName = a.Therapist?.Name ?? "—",
                Summary = $"{a.AssessmentName} — {a.Category}" +
                           (a.StandardScore.HasValue ? $" (الدرجة: {a.StandardScore})" : ""),
                MasteryPercent = a.StandardScore,
                Status = a.SeverityLevel ?? ""
            });
        }

        // From plans progress
        foreach (var plan in plans)
        {
            foreach (var obj in plan.Objectives)
            {
                foreach (var prog in obj.ProgressRecords.OrderByDescending(p => p.Date))
                {
                    decimal? mastery = null;
                    if (obj.Target.HasValue && obj.Target > 0 && prog.Score.HasValue)
                        mastery = Math.Round((prog.Score.Value / obj.Target.Value) * 100, 1);

                    historyList.Add(new HistoryEntry
                    {
                        Date = prog.Date,
                        EntryType = "تقدم الأهداف",
                        TypeIcon = "TrendingUp",
                        TypeColor = "#9f7aea",
                        TherapistName = "—",
                        Summary = $"{obj.ObjectiveText}" +
                                  (prog.Score.HasValue ? $" — الدرجة: {prog.Score}" : ""),
                        MasteryPercent = mastery,
                        Status = obj.Status
                    });
                }
            }
        }

        History.Clear();
        foreach (var h in historyList.OrderByDescending(x => x.Date))
            History.Add(h);

        // Stats
        TotalSessions = Beneficiary.Sessions?.Count(s => !s.IsDeleted) ?? 0;
        TotalAssessments = assessments.Count;
        ActivePlans = plans.Count(p => p.Status == "Active");
        var masteries = historyList
            .Where(h => h.MasteryPercent.HasValue && h.MasteryPercent > 0)
            .Select(h => h.MasteryPercent!.Value)
            .ToList();
        AvgMastery = masteries.Count > 0 ? Math.Round(masteries.Average(), 1) : 0;
    }

    // ── Photo ─────────────────────────────────────────────────────
    private async Task PickPhotoAsync()
    {
        var dialog = new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "اختر صورة المستفيد",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new Avalonia.Platform.Storage.FilePickerFileType("صور")
                {
                    Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" }
                }
            }
        };

        var topLevel = Helpers.TopLevelHelper.GetTopLevel();
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(dialog);
        if (files.Count == 0) return;

        var src = files[0].Path.LocalPath;
        var destDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RehabCenterApp", "Photos");
        Directory.CreateDirectory(destDir);
        var ext = Path.GetExtension(src);
        var dest = Path.Combine(destDir, $"beneficiary_{Beneficiary.Id}_{DateTime.Now:yyyyMMddHHmmss}{ext}");
        File.Copy(src, dest, overwrite: true);

        PhotoPath = dest;
        Beneficiary.PhotoPath = dest;
        await _dbService.UpdateBeneficiaryAsync(Beneficiary);
        this.RaisePropertyChanged(nameof(HasPhoto));
    }

    // ── Attachments ───────────────────────────────────────────────
    private async Task AddAttachmentAsync()
    {
        var dialog = new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "إرفاق ملف",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new Avalonia.Platform.Storage.FilePickerFileType("جميع الملفات")
                {
                    Patterns = new[] { "*.pdf", "*.jpg", "*.jpeg", "*.png", "*.doc", "*.docx", "*.xls", "*.xlsx" }
                }
            }
        };

        var topLevel = Helpers.TopLevelHelper.GetTopLevel();
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(dialog);
        if (files.Count == 0) return;

        var destDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RehabCenterApp", "Attachments", $"beneficiary_{Beneficiary.Id}");
        Directory.CreateDirectory(destDir);

        foreach (var file in files)
        {
            var src = file.Path.LocalPath;
            var fileName = Path.GetFileName(src);
            var dest = Path.Combine(destDir, $"{DateTime.Now:yyyyMMddHHmmss}_{fileName}");
            File.Copy(src, dest, overwrite: true);

            var ext = Path.GetExtension(fileName).ToLower();
            var attachType = ext switch
            {
                ".pdf" => "MedicalReport",
                ".jpg" or ".jpeg" or ".png" => "Photo",
                ".doc" or ".docx" => "Diagnosis",
                _ => "Other"
            };

            var attachment = new BeneficiaryAttachment
            {
                BeneficiaryId = Beneficiary.Id,
                FileName = fileName,
                FilePath = dest,
                AttachmentType = attachType,
                AddedByUser = UserSessionService.Instance.FullName,
                UploadDate = DateTime.Now
            };
            await _dbService.AddBeneficiaryAttachmentAsync(attachment);
            Attachments.Insert(0, attachment);
        }
    }

    private void OpenAttachment(BeneficiaryAttachment attachment)
    {
        if (!File.Exists(attachment.FilePath)) return;
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = attachment.FilePath,
                UseShellExecute = true
            });
        }
        catch { }
    }

    private async Task DeleteAttachmentAsync(BeneficiaryAttachment attachment)
    {
        var ok = await _dialogService.ShowConfirmAsync("تأكيد", $"حذف المرفق '{attachment.FileName}'؟");
        if (!ok) return;
        await _dbService.DeleteBeneficiaryAttachmentAsync(attachment.Id);
        Attachments.Remove(attachment);
    }

    // ── Save ──────────────────────────────────────────────────────
    private async Task SaveBasicInfoAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;
        Beneficiary.Name = Name;
        Beneficiary.DateOfBirth = DateOfBirth;
        Beneficiary.Gender = Gender;
        Beneficiary.NationalId = NationalId;
        Beneficiary.Phone = Phone;
        Beneficiary.Address = Address;
        Beneficiary.DisabilityType = DisabilityType;
        Beneficiary.GuardianName = GuardianName;
        Beneficiary.GuardianPhone = GuardianPhone;
        Beneficiary.GuardianRelation = GuardianRelation;
        Beneficiary.GuardianEmail = GuardianEmail;
        Beneficiary.EmergencyPhone = EmergencyPhone;
        Beneficiary.SchoolName = SchoolName;
        Beneficiary.ReferralSource = ReferralSource;
        Beneficiary.Status = Status;
        await _dbService.UpdateBeneficiaryAsync(Beneficiary);
        await _dialogService.ShowInfoAsync("تم الحفظ", "تم حفظ البيانات الأساسية بنجاح");
    }

    private async Task SaveMedicalAsync()
    {
        Beneficiary.Diagnosis = Diagnosis;
        Beneficiary.SecondaryDiagnosis = SecondaryDiagnosis;
        Beneficiary.MedicalHistory = MedicalHistory;
        Beneficiary.Allergies = Allergies;
        Beneficiary.BloodType = BloodType;
        Beneficiary.CurrentMedications = CurrentMedications;
        Beneficiary.FunctionalLevel = FunctionalLevel;
        Beneficiary.InsuranceCompany = InsuranceCompany;
        Beneficiary.InsuranceNumber = InsuranceNumber;
        await _dbService.UpdateBeneficiaryAsync(Beneficiary);
        await _dialogService.ShowInfoAsync("تم الحفظ", "تم حفظ المعلومات الطبية بنجاح");
    }
}
