using System;
using System.ComponentModel.DataAnnotations;

namespace RehabCenterApp.Models;

public class User : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(50)]
    public string Role { get; set; } = "Receptionist";

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public bool RequireLanAccess { get; set; } = true;

    public DateTime? LastLogin { get; set; }
}