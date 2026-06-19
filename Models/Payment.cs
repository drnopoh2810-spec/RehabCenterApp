using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RehabCenterApp.Models;

public class Payment : BaseEntity
{
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string PaymentType { get; set; } = "Cash";

    [MaxLength(50)]
    public string ReceiptNumber { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Paid";
}