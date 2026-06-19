using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RehabCenterApp.Models;

public class Session : BaseEntity
{
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    [MaxLength(50)]
    public string SessionType { get; set; } = string.Empty;

    public int TherapistId { get; set; }

    [ForeignKey("TherapistId")]
    public Employee Therapist { get; set; } = null!;

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public int Duration { get; set; } = 45;

    [MaxLength(20)]
    public string Status { get; set; } = "Scheduled";

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? Rating { get; set; }
}