using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RehabCenterApp.Models;

public class Beneficiary : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    [MaxLength(20)]
    public string Gender { get; set; } = "Male";

    [MaxLength(20)]
    public string? NationalId { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(50)]
    public string DisabilityType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Diagnosis { get; set; }

    [MaxLength(100)]
    public string? GuardianName { get; set; }

    [MaxLength(20)]
    public string? GuardianPhone { get; set; }

    [MaxLength(100)]
    public string? InsuranceCompany { get; set; }

    [MaxLength(50)]
    public string? InsuranceNumber { get; set; }

    public string? PhotoPath { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.Now;

    [MaxLength(20)]
    public string Status { get; set; } = "Active";

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    [NotMapped]
    public int Age => DateTime.Now.Year - DateOfBirth.Year;

    [NotMapped]
    public decimal TotalPaid => Payments.Sum(p => p.Amount);

    [NotMapped]
    public Session? LastSession => Sessions.OrderByDescending(s => s.Date).FirstOrDefault();
}