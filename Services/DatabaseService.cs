using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RehabCenterApp.Models;
using RehabCenterApp.Helpers;

namespace RehabCenterApp.Services;

public class DatabaseService
{
    private readonly AppDbContext _context;

    public DatabaseService(AppDbContext context)
    {
        _context = context;
        _context.Database.EnsureCreated();
        RunMigrations();
    }

    /// <summary>
    /// Safe runtime migrations — adds new columns to existing tables without breaking existing data.
    /// SQLite doesn't support IF NOT EXISTS in ALTER TABLE, so we catch the exception.
    /// </summary>
    private void RunMigrations()
    {
        var conn = _context.Database.GetDbConnection();
        try
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            void TryAdd(string table, string column, string type)
            {
                try
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {type}";
                    cmd.ExecuteNonQuery();
                }
                catch { /* column already exists — ignore */ }
            }

            // User table new columns
            TryAdd("Users", "FullName",         "TEXT");
            TryAdd("Users", "Phone",            "TEXT");
            TryAdd("Users", "Email",            "TEXT");
            TryAdd("Users", "Notes",            "TEXT");
            TryAdd("Users", "RequireLanAccess", "INTEGER NOT NULL DEFAULT 1");

            // Beneficiary table new columns
            TryAdd("Beneficiaries", "GuardianRelation",    "TEXT");
            TryAdd("Beneficiaries", "GuardianNationalId",  "TEXT");
            TryAdd("Beneficiaries", "GuardianEmail",       "TEXT");
            TryAdd("Beneficiaries", "EmergencyPhone",      "TEXT");
            TryAdd("Beneficiaries", "SchoolName",          "TEXT");
            TryAdd("Beneficiaries", "ReferralSource",      "TEXT");
            TryAdd("Beneficiaries", "SecondaryDiagnosis",  "TEXT");
            TryAdd("Beneficiaries", "MedicalHistory",      "TEXT");
            TryAdd("Beneficiaries", "Allergies",           "TEXT");
            TryAdd("Beneficiaries", "BloodType",           "TEXT");
            TryAdd("Beneficiaries", "CurrentMedications",  "TEXT");
            TryAdd("Beneficiaries", "FunctionalLevel",     "TEXT");
        }
        catch { /* ignore migration errors — app still works */ }
    }

    // Beneficiaries
    public async Task<List<Beneficiary>> GetBeneficiariesAsync(string? search = null)
    {
        var query = _context.Beneficiaries
            .Include(b => b.Sessions)
            .Include(b => b.Payments)
            .Where(b => !b.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(b => b.Name.Contains(search) || 
                                     (b.Phone != null && b.Phone.Contains(search)) ||
                                     (b.NationalId != null && b.NationalId.Contains(search)));
        }

        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    public async Task<List<Beneficiary>> SearchBeneficiariesFuzzyAsync(string search)
    {
        var all = await _context.Beneficiaries.Where(b => !b.IsDeleted).ToListAsync();
        return all.Where(b => LevenshteinDistance.Similarity(b.Name, search) > 0.6 ||
                               b.Name.Contains(search))
                  .OrderByDescending(b => LevenshteinDistance.Similarity(b.Name, search))
                  .ToList();
    }

    public async Task<Beneficiary?> GetBeneficiaryByIdAsync(int id)
    {
        return await _context.Beneficiaries
            .Include(b => b.Sessions)
            .ThenInclude(s => s.Therapist)
            .Include(b => b.Payments)
            .Include(b => b.Reminders)
            .Include(b => b.Attachments)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
    }

    // Beneficiary Attachments
    public async Task<List<BeneficiaryAttachment>> GetBeneficiaryAttachmentsAsync(int beneficiaryId)
    {
        return await _context.BeneficiaryAttachments
            .Where(a => a.BeneficiaryId == beneficiaryId && !a.IsDeleted)
            .OrderByDescending(a => a.UploadDate)
            .ToListAsync();
    }

    public async Task AddBeneficiaryAttachmentAsync(BeneficiaryAttachment attachment)
    {
        _context.BeneficiaryAttachments.Add(attachment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBeneficiaryAttachmentAsync(int id)
    {
        var a = await _context.BeneficiaryAttachments.FindAsync(id);
        if (a != null) { a.IsDeleted = true; await _context.SaveChangesAsync(); }
    }

    // Beneficiary full history for profile view
    public async Task<List<Assessment>> GetBeneficiaryAssessmentsAsync(int beneficiaryId)
    {
        return await _context.Assessments
            .Include(a => a.Therapist)
            .Where(a => a.BeneficiaryId == beneficiaryId && !a.IsDeleted)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync();
    }

    public async Task<List<InterventionPlan>> GetBeneficiaryPlansAsync(int beneficiaryId)
    {
        return await _context.InterventionPlans
            .Include(p => p.Objectives)
            .ThenInclude(o => o.ProgressRecords)
            .Where(p => p.BeneficiaryId == beneficiaryId && !p.IsDeleted)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();
    }

    public async Task<List<TherapistReport>> GetBeneficiaryTherapistReportsAsync(int beneficiaryId)
    {
        return await _context.TherapistReports
            .Include(r => r.Session)
            .Include(r => r.Therapist)
            .Where(r => r.Session.BeneficiaryId == beneficiaryId && !r.IsDeleted)
            .OrderByDescending(r => r.ReportDate)
            .ToListAsync();
    }

    public async Task<Beneficiary> AddBeneficiaryAsync(Beneficiary beneficiary)
    {
        _context.Beneficiaries.Add(beneficiary);
        await _context.SaveChangesAsync();
        return beneficiary;
    }

    public async Task UpdateBeneficiaryAsync(Beneficiary beneficiary)
    {
        beneficiary.UpdatedAt = DateTime.Now;
        _context.Beneficiaries.Update(beneficiary);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBeneficiaryAsync(int id)
    {
        var beneficiary = await _context.Beneficiaries.FindAsync(id);
        if (beneficiary != null)
        {
            beneficiary.IsDeleted = true;
            beneficiary.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetBeneficiariesCountAsync()
    {
        return await _context.Beneficiaries.CountAsync(b => !b.IsDeleted);
    }

    // Sessions
    public async Task<List<Session>> GetSessionsAsync(DateTime? date = null, int? beneficiaryId = null)
    {
        var query = _context.Sessions
            .Include(s => s.Beneficiary)
            .Include(s => s.Therapist)
            .Where(s => !s.IsDeleted)
            .AsNoTracking();

        if (date.HasValue)
            query = query.Where(s => s.Date.Date == date.Value.Date);

        if (beneficiaryId.HasValue)
            query = query.Where(s => s.BeneficiaryId == beneficiaryId.Value);

        return await query.OrderBy(s => s.Date).ThenBy(s => s.Time).ToListAsync();
    }

    public async Task<List<Session>> GetTodaySessionsAsync()
    {
        return await GetSessionsAsync(DateTime.Now);
    }

    public async Task<Session> AddSessionAsync(Session session)
    {
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task UpdateSessionAsync(Session session)
    {
        session.UpdatedAt = DateTime.Now;
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetTodaySessionsCountAsync()
    {
        return await _context.Sessions.CountAsync(s => s.Date.Date == DateTime.Now.Date && !s.IsDeleted);
    }

    // Payments
    public async Task<List<Payment>> GetPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null, int? beneficiaryId = null)
    {
        var query = _context.Payments
            .Include(p => p.Beneficiary)
            .Where(p => !p.IsDeleted)
            .AsNoTracking();

        if (startDate.HasValue)
            query = query.Where(p => p.Date >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(p => p.Date <= endDate.Value);
        if (beneficiaryId.HasValue)
            query = query.Where(p => p.BeneficiaryId == beneficiaryId.Value);

        return await query.OrderByDescending(p => p.Date).ToListAsync();
    }

    public async Task<Payment> AddPaymentAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Payments.Where(p => !p.IsDeleted);
        if (startDate.HasValue) query = query.Where(p => p.Date >= startDate.Value);
        if (endDate.HasValue) query = query.Where(p => p.Date <= endDate.Value);
        return await query.SumAsync(p => p.Amount);
    }

    // Expenses
    public async Task<List<Expense>> GetExpensesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Expenses.Where(e => !e.IsDeleted).AsNoTracking();
        if (startDate.HasValue) query = query.Where(e => e.Date >= startDate.Value);
        if (endDate.HasValue) query = query.Where(e => e.Date <= endDate.Value);
        return await query.OrderByDescending(e => e.Date).ToListAsync();
    }

    public async Task<Expense> AddExpenseAsync(Expense expense)
    {
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        return expense;
    }

    public async Task<decimal> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Expenses.Where(e => !e.IsDeleted);
        if (startDate.HasValue) query = query.Where(e => e.Date >= startDate.Value);
        if (endDate.HasValue) query = query.Where(e => e.Date <= endDate.Value);
        return await query.SumAsync(e => e.Amount);
    }

    // Reminders
    public async Task<List<Reminder>> GetRemindersAsync(string? status = null)
    {
        var query = _context.Reminders
            .Include(r => r.Beneficiary)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        return await query.OrderBy(r => r.DateTime).ToListAsync();
    }

    public async Task<List<Reminder>> GetUpcomingRemindersAsync(int days = 7)
    {
        var endDate = DateTime.Now.AddDays(days);
        return await _context.Reminders
            .Include(r => r.Beneficiary)
            .Where(r => !r.IsDeleted && r.DateTime <= endDate && r.Status == "Pending")
            .OrderBy(r => r.DateTime)
            .ToListAsync();
    }

    public async Task<Reminder> AddReminderAsync(Reminder reminder)
    {
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();
        return reminder;
    }

    public async Task UpdateReminderAsync(Reminder reminder)
    {
        reminder.UpdatedAt = DateTime.Now;
        _context.Reminders.Update(reminder);
        await _context.SaveChangesAsync();
    }

    // Employees
    public async Task<List<Employee>> GetEmployeesAsync(string? role = null)
    {
        var query = _context.Employees.Where(e => !e.IsDeleted).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(e => e.Role == role);
        return await query.OrderBy(e => e.Name).ToListAsync();
    }

    public async Task<Employee> AddEmployeeAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        employee.UpdatedAt = DateTime.Now;
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task<List<EmployeePayroll>> GetPayrollsAsync(int? employeeId = null, int? month = null, int? year = null)
    {
        var query = _context.EmployeePayrolls
            .Include(p => p.Employee)
            .Where(p => !p.IsDeleted)
            .AsNoTracking();
        if (employeeId.HasValue)
            query = query.Where(p => p.EmployeeId == employeeId.Value);
        if (month.HasValue)
            query = query.Where(p => p.Month == month.Value);
        if (year.HasValue)
            query = query.Where(p => p.Year == year.Value);
        return await query.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToListAsync();
    }

    public async Task<EmployeePayroll> AddPayrollAsync(EmployeePayroll payroll)
    {
        _context.EmployeePayrolls.Add(payroll);
        await _context.SaveChangesAsync();
        return payroll;
    }

    public async Task UpdatePayrollAsync(EmployeePayroll payroll)
    {
        payroll.UpdatedAt = DateTime.Now;
        _context.EmployeePayrolls.Update(payroll);
        await _context.SaveChangesAsync();
    }

    public async Task<List<EmployeeAttendance>> GetMonthlyAttendanceAsync(int employeeId, int month, int year)
    {
        return await _context.EmployeeAttendances
            .Where(a => !a.IsDeleted && a.EmployeeId == employeeId
                && a.Date.Month == month && a.Date.Year == year)
            .OrderBy(a => a.Date)
            .ToListAsync();
    }

    public async Task UpdateEmployeeAttendanceAsync(EmployeeAttendance attendance)
    {
        attendance.UpdatedAt = DateTime.Now;
        _context.EmployeeAttendances.Update(attendance);
        await _context.SaveChangesAsync();
    }

    // Users
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive && !u.IsDeleted);
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.Role).ThenBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<User> AddUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.Now;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    // Correspondence
    public async Task<List<Correspondence>> GetCorrespondencesAsync(string? type = null, string? search = null)
    {
        var query = _context.Correspondences.Where(c => !c.IsDeleted).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(type) && type != "All")
            query = query.Where(c => c.Type == type);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Number.Contains(search) || c.Subject.Contains(search) || c.Entity.Contains(search));
        return await query.OrderByDescending(c => c.Date).ToListAsync();
    }

    public async Task<Correspondence> AddCorrespondenceAsync(Correspondence correspondence)
    {
        _context.Correspondences.Add(correspondence);
        await _context.SaveChangesAsync();
        return correspondence;
    }

    public async Task UpdateCorrespondenceAsync(Correspondence correspondence)
    {
        correspondence.UpdatedAt = DateTime.Now;
        _context.Correspondences.Update(correspondence);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCorrespondenceAsync(int id)
    {
        var item = await _context.Correspondences.FindAsync(id);
        if (item != null)
        {
            item.IsDeleted = true;
            item.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    // Settings
    public async Task<string?> GetSettingAsync(string key)
    {
        var setting = await _context.AppSettings.FindAsync(key);
        return setting?.Value;
    }

    public async Task SetSettingAsync(string key, string value)
    {
        var setting = await _context.AppSettings.FindAsync(key);
        if (setting != null)
        {
            setting.Value = value;
        }
        else
        {
            _context.AppSettings.Add(new AppSetting { Key = key, Value = value });
        }
        await _context.SaveChangesAsync();
    }

    // Dashboard Stats
    public async Task<(int beneficiaries, int sessions, decimal revenue, decimal expenses)> GetDashboardStatsAsync()
    {
        var today = DateTime.Now.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var beneficiaries = await _context.Beneficiaries.CountAsync(b => !b.IsDeleted);
        var sessions = await _context.Sessions.CountAsync(s => s.Date.Date == today && !s.IsDeleted);
        var revenue = await _context.Payments
            .Where(p => p.Date >= monthStart && !p.IsDeleted)
            .SumAsync(p => p.Amount);
        var expenses = await _context.Expenses
            .Where(e => e.Date >= monthStart && !e.IsDeleted)
            .SumAsync(e => e.Amount);

        return (beneficiaries, sessions, revenue, expenses);
    }

    // Reports
    public async Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int year)
    {
        var payments = await _context.Payments
            .Where(p => p.Date.Year == year && !p.IsDeleted)
            .ToListAsync();

        return payments.GroupBy(p => p.Date.Month)
                       .ToDictionary(g => g.Key.ToString(), g => g.Sum(p => p.Amount));
    }

    public async Task<Dictionary<string, int>> GetDisabilityDistributionAsync()
    {
        var beneficiaries = await _context.Beneficiaries.Where(b => !b.IsDeleted).ToListAsync();
        return beneficiaries.GroupBy(b => b.DisabilityType)
                            .ToDictionary(g => g.Key, g => g.Count());
    }

    // Assessments
    public async Task<List<Assessment>> GetAssessmentsAsync(int? beneficiaryId = null)
    {
        var query = _context.Assessments
            .Include(a => a.Beneficiary)
            .Include(a => a.Therapist)
            .Where(a => !a.IsDeleted)
            .AsNoTracking();

        if (beneficiaryId.HasValue)
            query = query.Where(a => a.BeneficiaryId == beneficiaryId.Value);

        return await query.OrderByDescending(a => a.AssessmentDate).ToListAsync();
    }

    public async Task<List<Assessment>> GetAssessmentHistoryAsync(int beneficiaryId, string assessmentName)
    {
        return await _context.Assessments
            .Where(a => a.BeneficiaryId == beneficiaryId && a.AssessmentName == assessmentName && !a.IsDeleted)
            .OrderBy(a => a.AssessmentDate)
            .ToListAsync();
    }

    public async Task<Assessment> AddAssessmentAsync(Assessment assessment)
    {
        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();
        return assessment;
    }

    // Intervention Plans
    public async Task<List<InterventionPlan>> GetInterventionPlansAsync(int? beneficiaryId = null)
    {
        var query = _context.InterventionPlans
            .Include(p => p.Beneficiary)
            .Include(p => p.Objectives)
            .Where(p => !p.IsDeleted)
            .AsNoTracking();

        if (beneficiaryId.HasValue)
            query = query.Where(p => p.BeneficiaryId == beneficiaryId.Value);

        return await query.OrderByDescending(p => p.StartDate).ToListAsync();
    }

    public async Task<InterventionPlan> AddInterventionPlanAsync(InterventionPlan plan)
    {
        _context.InterventionPlans.Add(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task<PlanObjective> AddPlanObjectiveAsync(PlanObjective objective)
    {
        _context.PlanObjectives.Add(objective);
        await _context.SaveChangesAsync();
        return objective;
    }

    public async Task<ObjectiveProgress> AddObjectiveProgressAsync(ObjectiveProgress progress)
    {
        _context.ObjectiveProgresses.Add(progress);
        await _context.SaveChangesAsync();
        return progress;
    }

    // Waiting List
    public async Task<List<WaitingListEntry>> GetWaitingListAsync(string? status = null)
    {
        var query = _context.WaitingListEntries
            .Where(e => !e.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(e => e.Status == status);

        return await query.OrderBy(e => e.Priority).ThenBy(e => e.RegistrationDate).ToListAsync();
    }

    public async Task<WaitingListEntry> AddWaitingListEntryAsync(WaitingListEntry entry)
    {
        _context.WaitingListEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task UpdateWaitingListEntryAsync(WaitingListEntry entry)
    {
        entry.UpdatedAt = DateTime.Now;
        _context.WaitingListEntries.Update(entry);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteWaitingListEntryAsync(int id)
    {
        var entry = await _context.WaitingListEntries.FindAsync(id);
        if (entry != null)
        {
            entry.IsDeleted = true;
            entry.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    // Inventory
    public async Task<List<InventoryItem>> GetInventoryItemsAsync(string? category = null)
    {
        var query = _context.InventoryItems
            .Where(i => !i.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(i => i.Category == category);

        return await query.OrderBy(i => i.Name).ToListAsync();
    }

    public async Task<InventoryItem> AddInventoryItemAsync(InventoryItem item)
    {
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateInventoryItemAsync(InventoryItem item)
    {
        item.UpdatedAt = DateTime.Now;
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task<InventoryTransaction> AddInventoryTransactionAsync(InventoryTransaction transaction)
    {
        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    // Parent Communications
    public async Task<List<ParentCommunication>> GetParentCommunicationsAsync(int? beneficiaryId = null)
    {
        var query = _context.ParentCommunications
            .Include(c => c.Beneficiary)
            .Where(c => !c.IsDeleted)
            .AsNoTracking();

        if (beneficiaryId.HasValue)
            query = query.Where(c => c.BeneficiaryId == beneficiaryId.Value);

        return await query.OrderByDescending(c => c.Date).ToListAsync();
    }

    public async Task<ParentCommunication> AddParentCommunicationAsync(ParentCommunication communication)
    {
        _context.ParentCommunications.Add(communication);
        await _context.SaveChangesAsync();
        return communication;
    }

    // MDT Meetings
    public async Task<List<MDTMeeting>> GetMDTMeetingsAsync(int? beneficiaryId = null)
    {
        var query = _context.MDTMeetings
            .Include(m => m.Beneficiary)
            .Include(m => m.Attendees)
            .ThenInclude(a => a.Employee)
            .Where(m => !m.IsDeleted)
            .AsNoTracking();

        if (beneficiaryId.HasValue)
            query = query.Where(m => m.BeneficiaryId == beneficiaryId.Value);

        return await query.OrderByDescending(m => m.MeetingDate).ToListAsync();
    }

    public async Task<MDTMeeting> AddMDTMeetingAsync(MDTMeeting meeting)
    {
        _context.MDTMeetings.Add(meeting);
        await _context.SaveChangesAsync();
        return meeting;
    }

    public async Task UpdateMDTMeetingAsync(MDTMeeting meeting)
    {
        meeting.UpdatedAt = DateTime.Now;
        _context.MDTMeetings.Update(meeting);
        await _context.SaveChangesAsync();
    }

    public async Task<MDTAttendee> AddMDTAttendeeAsync(MDTAttendee attendee)
    {
        _context.MDTAttendees.Add(attendee);
        await _context.SaveChangesAsync();
        return attendee;
    }

    // Employee Attendance
    public async Task<List<EmployeeAttendance>> GetEmployeeAttendanceAsync(int? employeeId = null, DateTime? date = null)
    {
        var query = _context.EmployeeAttendances
            .Include(a => a.Employee)
            .Where(a => !a.IsDeleted)
            .AsNoTracking();

        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        if (date.HasValue)
            query = query.Where(a => a.Date.Date == date.Value.Date);

        return await query.OrderByDescending(a => a.Date).ToListAsync();
    }

    public async Task<EmployeeAttendance> AddEmployeeAttendanceAsync(EmployeeAttendance attendance)
    {
        _context.EmployeeAttendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance;
    }

    // Progress Predictions
    public async Task<List<ProgressPrediction>> GetProgressPredictionsAsync(int? beneficiaryId = null)
    {
        var query = _context.ProgressPredictions
            .Include(p => p.Beneficiary)
            .Where(p => !p.IsDeleted)
            .AsNoTracking();
        if (beneficiaryId.HasValue) query = query.Where(p => p.BeneficiaryId == beneficiaryId.Value);
        return await query.OrderByDescending(p => p.PredictionDate).ToListAsync();
    }

    public async Task<ProgressPrediction> AddProgressPredictionAsync(ProgressPrediction prediction)
    {
        _context.ProgressPredictions.Add(prediction);
        await _context.SaveChangesAsync();
        return prediction;
    }

    // Telehealth
    public async Task<List<TelehealthSession>> GetTelehealthSessionsAsync(int? beneficiaryId = null)
    {
        var query = _context.TelehealthSessions
            .Include(t => t.Session)
            .ThenInclude(s => s.Beneficiary)
            .Where(t => !t.IsDeleted)
            .AsNoTracking();
        if (beneficiaryId.HasValue) query = query.Where(t => t.Session.BeneficiaryId == beneficiaryId.Value);
        return await query.OrderByDescending(t => t.Session.Date).ToListAsync();
    }

    public async Task<TelehealthSession> AddTelehealthSessionAsync(TelehealthSession session)
    {
        _context.TelehealthSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    // Clinical Reports
    public async Task<List<ClinicalReport>> GetClinicalReportsAsync(int? beneficiaryId = null)
    {
        var query = _context.ClinicalReports
            .Include(r => r.Beneficiary)
            .Include(r => r.PreparedBy)
            .Include(r => r.ApprovedBy)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();
        if (beneficiaryId.HasValue) query = query.Where(r => r.BeneficiaryId == beneficiaryId.Value);
        return await query.OrderByDescending(r => r.ReportDate).ToListAsync();
    }

    public async Task<ClinicalReport> AddClinicalReportAsync(ClinicalReport report)
    {
        _context.ClinicalReports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task UpdateClinicalReportAsync(ClinicalReport report)
    {
        report.UpdatedAt = DateTime.Now;
        _context.ClinicalReports.Update(report);
        await _context.SaveChangesAsync();
    }

    // Parent Surveys
    public async Task<List<ParentSurvey>> GetParentSurveysAsync(int? beneficiaryId = null)
    {
        var query = _context.ParentSurveys
            .Include(s => s.Beneficiary)
            .Where(s => !s.IsDeleted)
            .AsNoTracking();
        if (beneficiaryId.HasValue) query = query.Where(s => s.BeneficiaryId == beneficiaryId.Value);
        return await query.OrderByDescending(s => s.SurveyDate).ToListAsync();
    }

    public async Task<ParentSurvey> AddParentSurveyAsync(ParentSurvey survey)
    {
        _context.ParentSurveys.Add(survey);
        await _context.SaveChangesAsync();
        return survey;
    }

    // Achievements / Gamification
    public async Task<List<Achievement>> GetAchievementsAsync(int? beneficiaryId = null)
    {
        var query = _context.Achievements
            .Include(a => a.Beneficiary)
            .Where(a => !a.IsDeleted)
            .AsNoTracking();
        if (beneficiaryId.HasValue) query = query.Where(a => a.BeneficiaryId == beneficiaryId.Value);
        return await query.OrderByDescending(a => a.AchievementDate).ToListAsync();
    }

    public async Task<Achievement> AddAchievementAsync(Achievement achievement)
    {
        _context.Achievements.Add(achievement);
        await _context.SaveChangesAsync();
        return achievement;
    }

    // Government Reports
    public async Task<List<GovernmentReport>> GetGovernmentReportsAsync()
    {
        return await _context.GovernmentReports
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.PeriodEnd)
            .ToListAsync();
    }

    public async Task<GovernmentReport> AddGovernmentReportAsync(GovernmentReport report)
    {
        _context.GovernmentReports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    // Document Archive
    public async Task<List<DocumentArchive>> GetDocumentArchivesAsync(string? category = null)
    {
        var query = _context.DocumentArchives
            .Include(d => d.Beneficiary)
            .Where(d => !d.IsDeleted)
            .AsNoTracking();
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(d => d.Category == category);
        return await query.OrderByDescending(d => d.DocumentDate).ToListAsync();
    }

    public async Task<List<DocumentArchive>> SearchDocumentArchivesAsync(string query)
    {
        return await _context.DocumentArchives
            .Where(d => !d.IsDeleted && (d.Title.Contains(query) || d.OcrText.Contains(query) || d.Tags.Contains(query)))
            .OrderByDescending(d => d.DocumentDate)
            .ToListAsync();
    }

    public async Task<DocumentArchive> AddDocumentArchiveAsync(DocumentArchive doc)
    {
        _context.DocumentArchives.Add(doc);
        await _context.SaveChangesAsync();
        return doc;
    }

    public async Task UpdateDocumentArchiveAsync(DocumentArchive doc)
    {
        doc.UpdatedAt = DateTime.Now;
        _context.DocumentArchives.Update(doc);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDocumentArchiveAsync(int id)
    {
        var doc = await _context.DocumentArchives.FindAsync(id);
        if (doc != null)
        {
            doc.IsDeleted = true;
            doc.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    // Leave Requests
    public async Task<List<LeaveRequest>> GetLeaveRequestsAsync(int? employeeId = null)
    {
        var query = _context.LeaveRequests
            .Include(l => l.Employee)
            .Where(l => !l.IsDeleted)
            .AsNoTracking();
        if (employeeId.HasValue) query = query.Where(l => l.EmployeeId == employeeId.Value);
        return await query.OrderByDescending(l => l.StartDate).ToListAsync();
    }

    public async Task<LeaveRequest> AddLeaveRequestAsync(LeaveRequest leave)
    {
        _context.LeaveRequests.Add(leave);
        await _context.SaveChangesAsync();
        return leave;
    }

    public async Task UpdateLeaveRequestAsync(LeaveRequest leave)
    {
        leave.UpdatedAt = DateTime.Now;
        _context.LeaveRequests.Update(leave);
        await _context.SaveChangesAsync();
    }

    // Therapist Evaluations
    public async Task<List<TherapistEvaluation>> GetTherapistEvaluationsAsync(int? therapistId = null)
    {
        var query = _context.TherapistEvaluations
            .Include(e => e.Therapist)
            .Where(e => !e.IsDeleted)
            .AsNoTracking();
        if (therapistId.HasValue) query = query.Where(e => e.TherapistId == therapistId.Value);
        return await query.OrderByDescending(e => e.EvaluationPeriodEnd).ToListAsync();
    }

    public async Task<TherapistEvaluation> AddTherapistEvaluationAsync(TherapistEvaluation evaluation)
    {
        _context.TherapistEvaluations.Add(evaluation);
        await _context.SaveChangesAsync();
        return evaluation;
    }

    // Insurance Claims
    public async Task<List<InsuranceClaim>> GetInsuranceClaimsAsync(int? beneficiaryId = null)
    {
        var query = _context.InsuranceClaims
            .Include(c => c.Beneficiary)
            .Include(c => c.ClaimSessions)
            .Where(c => !c.IsDeleted)
            .AsNoTracking();
        if (beneficiaryId.HasValue) query = query.Where(c => c.BeneficiaryId == beneficiaryId.Value);
        return await query.OrderByDescending(c => c.SubmissionDate).ToListAsync();
    }

    public async Task<InsuranceClaim> AddInsuranceClaimAsync(InsuranceClaim claim)
    {
        _context.InsuranceClaims.Add(claim);
        await _context.SaveChangesAsync();
        return claim;
    }

    // Session Recordings
    public async Task<List<SessionRecording>> GetSessionRecordingsAsync(int? sessionId = null)
    {
        var query = _context.SessionRecordings
            .Include(r => r.Session)
            .ThenInclude(s => s.Beneficiary)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();
        if (sessionId.HasValue) query = query.Where(r => r.SessionId == sessionId.Value);
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<SessionRecording> AddSessionRecordingAsync(SessionRecording recording)
    {
        _context.SessionRecordings.Add(recording);
        await _context.SaveChangesAsync();
        return recording;
    }

    // Progress Photos
    public async Task<List<ProgressPhoto>> GetProgressPhotosAsync(int beneficiaryId)
    {
        return await _context.ProgressPhotos
            .Where(p => p.BeneficiaryId == beneficiaryId && !p.IsDeleted)
            .OrderBy(p => p.PhotoDate)
            .ToListAsync();
    }

    public async Task<ProgressPhoto> AddProgressPhotoAsync(ProgressPhoto photo)
    {
        _context.ProgressPhotos.Add(photo);
        await _context.SaveChangesAsync();
        return photo;
    }

    // Therapist Reports
    public async Task<List<TherapistReport>> GetTherapistReportsAsync(int? therapistId = null, int? beneficiaryId = null)
    {
        var query = _context.TherapistReports
            .Include(r => r.Session)
                .ThenInclude(s => s.Beneficiary)
            .Include(r => r.Session)
                .ThenInclude(s => s.Therapist)
            .Include(r => r.Therapist)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();

        if (therapistId.HasValue)
            query = query.Where(r => r.TherapistId == therapistId.Value);
        if (beneficiaryId.HasValue)
            query = query.Where(r => r.Session.BeneficiaryId == beneficiaryId.Value);

        return await query.OrderByDescending(r => r.ReportDate).ToListAsync();
    }

    public async Task<TherapistReport?> GetTherapistReportBySessionAsync(int sessionId)
    {
        return await _context.TherapistReports
            .Include(r => r.Session)
                .ThenInclude(s => s.Beneficiary)
            .Include(r => r.Therapist)
            .FirstOrDefaultAsync(r => r.SessionId == sessionId && !r.IsDeleted);
    }

    public async Task<TherapistReport> AddTherapistReportAsync(TherapistReport report)
    {
        _context.TherapistReports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task UpdateTherapistReportAsync(TherapistReport report)
    {
        report.UpdatedAt = DateTime.Now;
        _context.TherapistReports.Update(report);
        await _context.SaveChangesAsync();
    }

    // Session Worksheets
    public async Task<List<SessionWorksheet>> GetSessionWorksheetsAsync(int sessionId)
    {
        return await _context.SessionWorksheets
            .Where(w => w.SessionId == sessionId && !w.IsDeleted)
            .AsNoTracking()
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<SessionWorksheet> AddSessionWorksheetAsync(SessionWorksheet worksheet)
    {
        _context.SessionWorksheets.Add(worksheet);
        await _context.SaveChangesAsync();
        return worksheet;
    }

    public async Task UpdateSessionWorksheetAsync(SessionWorksheet worksheet)
    {
        worksheet.UpdatedAt = DateTime.Now;
        _context.SessionWorksheets.Update(worksheet);
        await _context.SaveChangesAsync();
    }

    // Child History (sessions + reports for a beneficiary)
    public async Task<List<Session>> GetChildHistorySessionsAsync(int beneficiaryId)
    {
        return await _context.Sessions
            .Include(s => s.Therapist)
            .Where(s => s.BeneficiaryId == beneficiaryId && !s.IsDeleted)
            .OrderByDescending(s => s.Date)
            .ThenByDescending(s => s.Time)
            .AsNoTracking()
            .ToListAsync();
    }

    // Get sessions for a specific therapist by employee linked user
    public async Task<List<Session>> GetTherapistSessionsAsync(int therapistId, DateTime? date = null)
    {
        var query = _context.Sessions
            .Include(s => s.Beneficiary)
            .Include(s => s.Therapist)
            .Where(s => s.TherapistId == therapistId && !s.IsDeleted)
            .AsNoTracking();

        if (date.HasValue)
            query = query.Where(s => s.Date.Date == date.Value.Date);

        return await query.OrderBy(s => s.Time).ToListAsync();
    }

    // Get employee linked to a user account (by matching name or username)
    public async Task<Employee?> GetEmployeeByUsernameAsync(string username)
    {
        var usernameLower = username.ToLower();
        var allTherapists = await _context.Employees
            .Where(e => e.IsActive && !e.IsDeleted && e.Role == "Therapist")
            .AsNoTracking()
            .ToListAsync();

        return allTherapists.FirstOrDefault(e =>
            e.Name.ToLower().Contains(usernameLower) ||
            usernameLower.Contains(e.Name.ToLower().Split(' ')[0]));
    }
}