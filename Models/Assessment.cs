using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RehabCenterApp.Models;

/// <summary>
/// Standardized assessment/evaluation for beneficiaries
/// Supports autism scales, IQ tests, motor assessments, etc.
/// </summary>
public class Assessment : BaseEntity
{
    [Required]
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string AssessmentName { get; set; } = string.Empty; // e.g., "GARS-2", "WISC-V", "GMFM"

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // Cognitive, Motor, Behavioral, Speech, Sensory

    public DateTime AssessmentDate { get; set; } = DateTime.Now;

    public int? TherapistId { get; set; }

    [ForeignKey("TherapistId")]
    public Employee? Therapist { get; set; }

    // Raw score
    public decimal? RawScore { get; set; }

    // Standardized score (percentile, T-score, etc.)
    public decimal? StandardScore { get; set; }

    [MaxLength(50)]
    public string? SeverityLevel { get; set; } // Mild, Moderate, Severe, Profound

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(2000)]
    public string? Recommendations { get; set; }

    // For tracking progress over time
    public int? PreviousAssessmentId { get; set; }

    // Sub-scores (stored as JSON-like structure in real implementation)
    [MaxLength(4000)]
    public string? SubScoresJson { get; set; }

    public string? AttachmentPath { get; set; }
}

/// <summary>
/// Individualized Education Plan / Intervention Plan
/// </summary>
public class InterventionPlan : BaseEntity
{
    [Required]
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string PlanTitle { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? LongTermGoals { get; set; }

    public DateTime StartDate { get; set; } = DateTime.Now;

    public DateTime? EndDate { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Active"; // Active, Completed, Revised, Cancelled

    public ICollection<PlanObjective> Objectives { get; set; } = new List<PlanObjective>();

    public ICollection<PlanReview> Reviews { get; set; } = new List<PlanReview>();
}

public class PlanObjective : BaseEntity
{
    public int InterventionPlanId { get; set; }

    [ForeignKey("InterventionPlanId")]
    public InterventionPlan InterventionPlan { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string ObjectiveText { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Domain { get; set; } = string.Empty; // Motor, Cognitive, Social, Communication, ADL

    [MaxLength(50)]
    public string MeasurementMethod { get; set; } = string.Empty; // Frequency, Duration, Accuracy, Independence

    public decimal? Baseline { get; set; }

    public decimal? Target { get; set; }

    [MaxLength(20)]
    public string Criteria { get; set; } = "3/5 trials"; // e.g., "80% accuracy over 3 sessions"

    [MaxLength(20)]
    public string Status { get; set; } = "Not Started"; // Not Started, In Progress, Achieved, Discontinued

    public ICollection<ObjectiveProgress> ProgressRecords { get; set; } = new List<ObjectiveProgress>();
}

public class ObjectiveProgress : BaseEntity
{
    public int PlanObjectiveId { get; set; }

    [ForeignKey("PlanObjectiveId")]
    public PlanObjective PlanObjective { get; set; } = null!;

    public DateTime Date { get; set; } = DateTime.Now;

    public decimal? Score { get; set; }

    [MaxLength(20)]
    public string? PerformanceLevel { get; set; } // Independent, Prompted, Hand-over-hand, No response

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? SessionId { get; set; }

    [ForeignKey("SessionId")]
    public Session? Session { get; set; }
}

public class PlanReview : BaseEntity
{
    public int InterventionPlanId { get; set; }

    [ForeignKey("InterventionPlanId")]
    public InterventionPlan InterventionPlan { get; set; } = null!;

    public DateTime ReviewDate { get; set; } = DateTime.Now;

    [MaxLength(2000)]
    public string? Findings { get; set; }

    [MaxLength(2000)]
    public string? Modifications { get; set; }

    public int? ReviewedById { get; set; }

    [ForeignKey("ReviewedById")]
    public Employee? ReviewedBy { get; set; }
}
