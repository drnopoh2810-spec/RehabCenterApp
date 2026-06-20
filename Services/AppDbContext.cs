using Microsoft.EntityFrameworkCore;
using RehabCenterApp.Models;
using System;

namespace RehabCenterApp.Services;

public class AppDbContext : DbContext
{
    // Core entities
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Correspondence> Correspondences => Set<Correspondence>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    // Assessment & Intervention
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<InterventionPlan> InterventionPlans => Set<InterventionPlan>();
    public DbSet<PlanObjective> PlanObjectives => Set<PlanObjective>();
    public DbSet<ObjectiveProgress> ObjectiveProgresses => Set<ObjectiveProgress>();
    public DbSet<PlanReview> PlanReviews => Set<PlanReview>();

    // Waiting List & Inventory
    public DbSet<WaitingListEntry> WaitingListEntries => Set<WaitingListEntry>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    // Communication & Attendance
    public DbSet<ParentCommunication> ParentCommunications => Set<ParentCommunication>();
    public DbSet<EmployeeAttendance> EmployeeAttendances => Set<EmployeeAttendance>();
    public DbSet<MDTMeeting> MDTMeetings => Set<MDTMeeting>();
    public DbSet<MDTAttendee> MDTAttendees => Set<MDTAttendee>();

    // Advanced Features
    public DbSet<ProgressPrediction> ProgressPredictions => Set<ProgressPrediction>();
    public DbSet<SessionRecording> SessionRecordings => Set<SessionRecording>();
    public DbSet<ProgressPhoto> ProgressPhotos => Set<ProgressPhoto>();
    public DbSet<ClinicalReport> ClinicalReports => Set<ClinicalReport>();
    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();
    public DbSet<ClaimSession> ClaimSessions => Set<ClaimSession>();
    public DbSet<TelehealthSession> TelehealthSessions => Set<TelehealthSession>();
    public DbSet<ParentSurvey> ParentSurveys => Set<ParentSurvey>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<GovernmentReport> GovernmentReports => Set<GovernmentReport>();
    public DbSet<DocumentArchive> DocumentArchives => Set<DocumentArchive>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<TherapistEvaluation> TherapistEvaluations => Set<TherapistEvaluation>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RehabCenterApp", "data.db");

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dbPath)!);

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1, 
                Username = "admin", 
                PasswordHash = "$2b$12$PM6vdyTw6K1jsB4AHiuX7O5LKhwW2uZktTDFqrTJ1BB.5w6vEGwFS",
                Role = "Admin",
                IsActive = true
            }
        );

        modelBuilder.Entity<AppSetting>().HasData(
            new AppSetting { Key = "CenterName", Value = "Rehab Center" },
            new AppSetting { Key = "CenterAddress", Value = "" },
            new AppSetting { Key = "CenterPhone", Value = "" },
            new AppSetting { Key = "CenterEmail", Value = "" },
            new AppSetting { Key = "Theme", Value = "Light" },
            new AppSetting { Key = "AutoBackup", Value = "True" },
            new AppSetting { Key = "WhatsAppEnabled", Value = "False" },
            new AppSetting { Key = "SMSApiKey", Value = "" }
        );
    }
}