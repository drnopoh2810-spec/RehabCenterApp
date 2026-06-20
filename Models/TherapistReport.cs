using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RehabCenterApp.Models;

public class TherapistReport : BaseEntity
{
    public int SessionId { get; set; }

    [ForeignKey("SessionId")]
    public Session Session { get; set; } = null!;

    public int TherapistId { get; set; }

    [ForeignKey("TherapistId")]
    public Employee Therapist { get; set; } = null!;

    public DateTime ReportDate { get; set; } = DateTime.Now;

    [MaxLength(2000)]
    public string ActivitiesPerformed { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string ChildResponse { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string GoalsAddressed { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Homework { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string BehaviorNotes { get; set; } = string.Empty;

    public int OverallRating { get; set; } = 3;

    [MaxLength(2000)]
    public string NextSessionPlan { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Status { get; set; } = "Draft";
}
