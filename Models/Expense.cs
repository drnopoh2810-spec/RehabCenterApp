using System;
using System.ComponentModel.DataAnnotations;

namespace RehabCenterApp.Models;

public class Expense : BaseEntity
{
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? AttachmentPath { get; set; }
}