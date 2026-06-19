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

    [MaxLength(50)]
    public string Role { get; set; } = "Receptionist";

    public bool IsActive { get; set; } = true;

    public DateTime? LastLogin { get; set; }
}