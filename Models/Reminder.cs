using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RehabCenterApp.Models;

public class Reminder : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime DateTime { get; set; }

    [MaxLength(50)]
    public string Type { get; set; } = "General";

    [MaxLength(20)]
    public string Priority { get; set; } = "Medium";

    public int? BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary? Beneficiary { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";
}