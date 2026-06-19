using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace RehabCenterApp.Models;

public class Employee : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Role { get; set; } = "Therapist";

    [MaxLength(20)]
    public string? Phone { get; set; }

    public decimal Salary { get; set; }

    public DateTime JoinDate { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}