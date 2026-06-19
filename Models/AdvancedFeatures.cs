using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RehabCenterApp.Models;

/// <summary>
/// AI-powered progress prediction and risk analysis
/// </summary>
public class ProgressPrediction : BaseEntity
{
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    [MaxLength(50)]
    public string Domain { get; set; } = string.Empty; // Motor, Cognitive, Speech, Social

    public DateTime PredictionDate { get; set; } = DateTime.Now;

    public decimal CurrentScore { get; set; }

    public decimal PredictedScore3Months { get; set; }

    public decimal PredictedScore6Months { get; set; }

    public decimal PredictedScore12Months { get; set; }

    [MaxLength(20)]
    public string RiskLevel { get; set; } = "Low"; // Low, Medium, High, Critical

    [MaxLength(1000)]
    public string? RiskFactors { get; set; }

    [MaxLength(1000)]
    public string? Recommendations { get; set; }

    public decimal ConfidenceLevel { get; set; } = 0.85m; // 0-1
}

/// <summary>
/// Session video recordings for review and supervision
/// </summary>
public class SessionRecording : BaseEntity
{
    public int SessionId { get; set; }

    [ForeignKey("SessionId")]
    public Session Session { get; set; } = null!;

    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public TimeSpan Duration { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Recorded"; // Recorded, Processing, Reviewed, Archived

    [MaxLength(2000)]
    public string? TherapistNotes { get; set; }

    [MaxLength(2000)]
    public string? SupervisorNotes { get; set; }

    public int? SupervisorId { get; set; }

    public DateTime? ReviewDate { get; set; }

    public int? Rating { get; set; } // 1-5 session quality
}

/// <summary>
/// Before/After photo documentation for visual progress tracking
/// </summary>
public class ProgressPhoto : BaseEntity
{
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // Posture, Motor, Facial, General

    [MaxLength(500)]
    public string PhotoPath { get; set; } = string.Empty;

    public DateTime PhotoDate { get; set; } = DateTime.Now;

    [MaxLength(20)]
    public string Type { get; set; } = "Before"; // Before, After, Progress

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? RelatedPhotoId { get; set; } // Links Before to After
}

/// <summary>
/// Clinical reports with digital signatures
/// </summary>
public class ClinicalReport : BaseEntity
{
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string ReportType { get; set; } = string.Empty; // Initial, Progress, Discharge, Referral

    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;

    public DateTime ReportDate { get; set; } = DateTime.Now;

    public int? PreparedById { get; set; }

    [ForeignKey("PreparedById")]
    public Employee? PreparedBy { get; set; }

    public int? ApprovedById { get; set; }

    [ForeignKey("ApprovedById")]
    public Employee? ApprovedBy { get; set; }

    [MaxLength(500)]
    public string? DigitalSignature { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Reviewed, Approved, Signed

    public DateTime? ApprovalDate { get; set; }

    [MaxLength(500)]
    public string? PdfPath { get; set; }
}

/// <summary>
/// Insurance claims and approvals tracking
/// </summary>
public class InsuranceClaim : BaseEntity
{
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    [MaxLength(50)]
    public string ClaimNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string InsuranceCompany { get; set; } = string.Empty;

    public decimal ClaimedAmount { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public DateTime SubmissionDate { get; set; } = DateTime.Now;

    public DateTime? ResponseDate { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Partial, Rejected

    [MaxLength(2000)]
    public string? RejectionReason { get; set; }

    [MaxLength(500)]
    public string? AttachmentPath { get; set; }

    public ICollection<ClaimSession> ClaimSessions { get; set; } = new List<ClaimSession>();
}

public class ClaimSession : BaseEntity
{
    public int InsuranceClaimId { get; set; }

    [ForeignKey("InsuranceClaimId")]
    public InsuranceClaim InsuranceClaim { get; set; } = null!;

    public int SessionId { get; set; }

    [ForeignKey("SessionId")]
    public Session Session { get; set; } = null!;

    public decimal SessionCost { get; set; }
}

/// <summary>
/// Telehealth / Remote sessions
/// </summary>
public class TelehealthSession : BaseEntity
{
    public int SessionId { get; set; }

    [ForeignKey("SessionId")]
    public Session Session { get; set; } = null!;

    [MaxLength(200)]
    public string Platform { get; set; } = string.Empty; // Zoom, Teams, Custom

    [MaxLength(500)]
    public string? MeetingLink { get; set; }

    [MaxLength(50)]
    public string? MeetingId { get; set; }

    [MaxLength(50)]
    public string? Passcode { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, Active, Completed, Cancelled

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [MaxLength(20)]
    public string ConnectionQuality { get; set; } = "Good"; // Excellent, Good, Fair, Poor

    [MaxLength(1000)]
    public string? TechnicalNotes { get; set; }
}

/// <summary>
/// Parent satisfaction surveys and feedback
/// </summary>
public class ParentSurvey : BaseEntity
{
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    public DateTime SurveyDate { get; set; } = DateTime.Now;

    public int? OverallSatisfaction { get; set; } // 1-5

    public int? TherapistCompetency { get; set; }

    public int? ProgressSatisfaction { get; set; }

    public int? FacilityQuality { get; set; }

    public int? CommunicationQuality { get; set; }

    public int? SchedulingConvenience { get; set; }

    [MaxLength(2000)]
    public string? Suggestions { get; set; }

    [MaxLength(2000)]
    public string? Complaints { get; set; }

    public bool WouldRecommend { get; set; } = false;

    [MaxLength(20)]
    public string Status { get; set; } = "Completed"; // Pending, Completed, FollowUp
}

/// <summary>
/// Gamification - Achievement badges and points for children
/// </summary>
public class Achievement : BaseEntity
{
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(50)]
    public string BadgeIcon { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // Motor, Cognitive, Social, Attendance

    public int Points { get; set; } = 10;

    public DateTime AchievementDate { get; set; } = DateTime.Now;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? SessionId { get; set; }

    public bool IsNotified { get; set; } = false;
}

/// <summary>
/// Government reporting templates and submissions
/// </summary>
public class GovernmentReport : BaseEntity
{
    [MaxLength(100)]
    public string ReportType { get; set; } = string.Empty; // Monthly, Quarterly, Annual

    [MaxLength(100)]
    public string Authority { get; set; } = string.Empty; // Ministry, Insurance, Municipality

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    [MaxLength(5000)]
    public string? GeneratedData { get; set; } // JSON data

    [MaxLength(500)]
    public string? PdfPath { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Generated, Submitted, Approved

    public DateTime? SubmissionDate { get; set; }

    [MaxLength(100)]
    public string? SubmittedBy { get; set; }

    [MaxLength(500)]
    public string? ConfirmationNumber { get; set; }
}

/// <summary>
/// Document archive with OCR indexing
/// </summary>
public class DocumentArchive : BaseEntity
{
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // Medical, Legal, Financial, Educational

    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty; // PDF, Image, Word, Excel

    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public int? BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary? Beneficiary { get; set; }

    [MaxLength(4000)]
    public string? OcrText { get; set; } // Extracted text for search

    [MaxLength(1000)]
    public string? Tags { get; set; }

    public DateTime DocumentDate { get; set; } = DateTime.Now;

    [MaxLength(20)]
    public string Confidentiality { get; set; } = "Normal"; // Normal, Confidential, Restricted

    public bool IsIndexed { get; set; } = false;
}

/// <summary>
/// HR - Employee leave requests and attendance
/// </summary>
public class LeaveRequest : BaseEntity
{
    public int EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee Employee { get; set; } = null!;

    [MaxLength(50)]
    public string LeaveType { get; set; } = string.Empty; // Annual, Sick, Emergency, Unpaid, Maternity

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int DaysCount => (EndDate - StartDate).Days + 1;

    [MaxLength(1000)]
    public string? Reason { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled

    public int? ApprovedById { get; set; }

    public DateTime? ApprovalDate { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}

/// <summary>
/// Therapist performance evaluation
/// </summary>
public class TherapistEvaluation : BaseEntity
{
    public int TherapistId { get; set; }

    [ForeignKey("TherapistId")]
    public Employee Therapist { get; set; } = null!;

    public DateTime EvaluationPeriodStart { get; set; }

    public DateTime EvaluationPeriodEnd { get; set; }

    public int? SessionsCount { get; set; }

    public int? BeneficiariesCount { get; set; }

    public decimal? AttendanceRate { get; set; }

    public decimal? ParentSatisfactionAvg { get; set; }

    public decimal? ProgressAchievementRate { get; set; }

    [MaxLength(2000)]
    public string? Strengths { get; set; }

    [MaxLength(2000)]
    public string? AreasForImprovement { get; set; }

    [MaxLength(2000)]
    public string? Goals { get; set; }

    public int? EvaluatedById { get; set; }

    [MaxLength(20)]
    public string OverallRating { get; set; } = "Good"; // Excellent, Good, Satisfactory, NeedsImprovement
}
