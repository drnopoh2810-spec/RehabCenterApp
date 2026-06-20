using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RehabCenterApp.Models;

public class BeneficiaryAttachment : BaseEntity
{
    [Required]
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string AttachmentType { get; set; } = "Other";
    // Photo, MedicalReport, Diagnosis, RehabPlan, Assessment, Other

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? AddedByUser { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.Now;
}
