using System;
using System.ComponentModel.DataAnnotations;

namespace RehabCenterApp.Models;

public class Correspondence : BaseEntity
{
    [MaxLength(10)]
    public string Type { get; set; } = "In";

    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.Now;

    [MaxLength(100)]
    public string Entity { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public string? AttachmentPath { get; set; }
}