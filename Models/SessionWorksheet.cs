using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RehabCenterApp.Models;

public class SessionWorksheet : BaseEntity
{
    public int SessionId { get; set; }

    [ForeignKey("SessionId")]
    public Session Session { get; set; } = null!;

    [MaxLength(100)]
    public string WorksheetType { get; set; } = "General";

    [MaxLength(4000)]
    public string ChecklistItems { get; set; } = "[]";

    [MaxLength(1000)]
    public string MaterialsUsed { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string ObservationNotes { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string PhotoPaths { get; set; } = string.Empty;
}
