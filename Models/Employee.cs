using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace RehabCenterApp.Models;

public class Employee : BaseEntity
{
    [MaxLength(20)]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Role { get; set; } = "Therapist";

    [MaxLength(100)]
    public string? Position { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? NationalId { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? EmergencyContact { get; set; }

    [MaxLength(20)]
    public string? EmergencyPhone { get; set; }

    public decimal BaseSalary { get; set; }

    public decimal Salary { get; set; }

    public DateTime JoinDate { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? PhotoPath { get; set; }

    [MaxLength(500)]
    public string? ContractPath { get; set; }

    [MaxLength(500)]
    public string? CvPath { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<EmployeePayroll> Payrolls { get; set; } = new List<EmployeePayroll>();
}

public class EmployeePayroll : BaseEntity
{
    public int EmployeeId { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("EmployeeId")]
    public Employee Employee { get; set; } = null!;

    public int Month { get; set; }

    public int Year { get; set; }

    public decimal BaseSalary { get; set; }

    public decimal HousingAllowance { get; set; }

    public decimal TransportAllowance { get; set; }

    public decimal OtherAllowances { get; set; }

    public decimal Incentives { get; set; }

    public decimal Deductions { get; set; }

    public int AbsenceDays { get; set; }

    public decimal AbsenceDeduction { get; set; }

    public decimal NetSalary =>
        BaseSalary + HousingAllowance + TransportAllowance + OtherAllowances + Incentives
        - Deductions - AbsenceDeduction;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime? PaidDate { get; set; }

    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
}
