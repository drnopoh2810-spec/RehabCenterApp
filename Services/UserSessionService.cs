using System;
using RehabCenterApp.Models;

namespace RehabCenterApp.Services;

/// <summary>
/// Singleton that holds the currently logged-in user for the session.
/// </summary>
public class UserSessionService
{
    private static UserSessionService? _instance;
    public static UserSessionService Instance => _instance ??= new UserSessionService();

    public int UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;

    public bool IsAdmin => Role == "Admin";
    public bool IsTherapist => Role == "Therapist";
    public bool IsReceptionist => Role == "Receptionist";
    public bool IsAccountant => Role == "Accountant";
    public bool IsHR => Role == "HR";
    public bool IsPR => Role == "PR";

    // ── Per-nav-section visibility ─────────────────────────────
    public bool CanSeeDashboard => true;
    public bool CanSeeBeneficiaries => IsAdmin || IsReceptionist || IsPR;
    public bool CanSeeWaitingList => IsAdmin || IsReceptionist;
    public bool CanSeeSessions => IsAdmin || IsReceptionist;
    public bool CanSeeTelehealth => IsAdmin || IsReceptionist;
    public bool CanSeeAssessments => IsAdmin;
    public bool CanSeeInterventionPlans => IsAdmin;
    public bool CanSeeClinicalReports => IsAdmin;
    public bool CanSeeMDT => IsAdmin;
    public bool CanSeeGamification => IsAdmin;
    public bool CanSeeAccounting => IsAdmin || IsAccountant;
    public bool CanSeeInventory => IsAdmin || IsAccountant;
    public bool CanSeeHR => IsAdmin || IsHR || IsAccountant;
    public bool CanSeeCorrespondence => IsAdmin || IsReceptionist || IsPR;
    public bool CanSeeParentPortal => IsAdmin || IsReceptionist || IsPR;
    public bool CanSeeReminders => IsAdmin || IsReceptionist || IsPR;
    public bool CanSeeDocuments => IsAdmin || IsPR;
    public bool CanSeeAnalytics => IsAdmin || IsAccountant;
    public bool CanSeeGovernmentReports => IsAdmin || IsAccountant;
    public bool CanSeeForms => IsAdmin || IsReceptionist || IsPR;
    public bool CanSeeSettings => IsAdmin;
    public bool CanSeeUserManagement => IsAdmin;

    public void SetUser(User user)
    {
        UserId = user.Id;
        Username = user.Username;
        FullName = user.FullName ?? user.Username;
        Role = user.Role;
    }

    public void Clear()
    {
        UserId = 0;
        Username = string.Empty;
        FullName = string.Empty;
        Role = string.Empty;
    }

    public string RoleDisplayName => Role switch
    {
        "Admin" => "مدير النظام",
        "Therapist" => "أخصائي",
        "Receptionist" => "موظف استقبال",
        "Accountant" => "محاسب",
        "HR" => "موارد بشرية",
        "PR" => "علاقات عامة",
        _ => Role
    };
}
