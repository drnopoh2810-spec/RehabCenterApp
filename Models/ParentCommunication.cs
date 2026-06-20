using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RehabCenterApp.Models;

/// <summary>
/// Communication log with parents/guardians
/// </summary>
public class ParentCommunication : BaseEntity
{
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    [MaxLength(20)]
    public string CommunicationType { get; set; } = "WhatsApp"; // WhatsApp, SMS, Email, Call, Meeting

    [MaxLength(20)]
    public string Direction { get; set; } = "Out"; // In, Out

    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.Now;

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Sent"; // Draft, Sent, Delivered, Read, Failed

    public bool IsReport { get; set; } = false; // Auto-generated session report

    public int? SessionId { get; set; }

    [MaxLength(500)]
    public string? AttachmentPath { get; set; }
}

/// <summary>
/// Employee attendance and leave tracking
/// </summary>
public class EmployeeAttendance : BaseEntity
{
    public int EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee Employee { get; set; } = null!;

    public DateTime Date { get; set; } = DateTime.Now;

    public TimeSpan? CheckIn { get; set; }

    public TimeSpan? CheckOut { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Present"; // Present, Absent, Late, EarlyLeave, OnLeave

    [MaxLength(50)]
    public string? LeaveType { get; set; } // Annual, Sick, Emergency, Unpaid

    [MaxLength(500)]
    public string? Notes { get; set; }

    public decimal? HoursWorked { get; set; }
}

/// <summary>
/// Multi-Disciplinary Team (MDT) Meeting records
/// </summary>
public class MDTMeeting : BaseEntity
{
    public int BeneficiaryId { get; set; }

    [ForeignKey("BeneficiaryId")]
    public Beneficiary Beneficiary { get; set; } = null!;

    public DateTime MeetingDate { get; set; } = DateTime.Now;

    [MaxLength(2000)]
    public string? DiscussionPoints { get; set; }

    [MaxLength(2000)]
    public string? Decisions { get; set; }

    [MaxLength(2000)]
    public string? ActionPlan { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

    public ICollection<MDTAttendee> Attendees { get; set; } = new List<MDTAttendee>();
}

public class MDTAttendee : BaseEntity
{
    public int MDTMeetingId { get; set; }

    [ForeignKey("MDTMeetingId")]
    public MDTMeeting MDTMeeting { get; set; } = null!;

    public int EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee Employee { get; set; } = null!;

    [MaxLength(100)]
    public string Role { get; set; } = string.Empty; // Chair, Member, Observer

    public bool Attended { get; set; } = false;

    [MaxLength(1000)]
    public string? Contribution { get; set; }
}
