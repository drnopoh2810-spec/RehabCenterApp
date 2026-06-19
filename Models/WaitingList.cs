using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RehabCenterApp.Models;

/// <summary>
/// Waiting list for new beneficiaries when capacity is full
/// </summary>
public class WaitingListEntry : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    [MaxLength(20)]
    public string Gender { get; set; } = "Male";

    [MaxLength(50)]
    public string DisabilityType { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? GuardianName { get; set; }

    [MaxLength(20)]
    public string? GuardianPhone { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.Now;

    public int Priority { get; set; } = 3; // 1=Urgent, 2=High, 3=Normal, 4=Low

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Waiting"; // Waiting, Contacted, Scheduled, Converted, Cancelled

    public DateTime? ContactDate { get; set; }

    public int? ConvertedToBeneficiaryId { get; set; }
}

/// <summary>
/// Inventory management for therapeutic equipment and supplies
/// </summary>
public class InventoryItem : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // Equipment, Supplies, Toys, Furniture

    [MaxLength(200)]
    public string? Description { get; set; }

    public int Quantity { get; set; } = 0;

    public int MinStockLevel { get; set; } = 5; // Alert when below this

    [MaxLength(50)]
    public string? Unit { get; set; } // Piece, Box, Set, kg

    public decimal? UnitCost { get; set; }

    public decimal? TotalValue => Quantity * UnitCost;

    [MaxLength(100)]
    public string? Supplier { get; set; }

    [MaxLength(50)]
    public string? Location { get; set; } // Room, Shelf

    public DateTime? PurchaseDate { get; set; }

    public DateTime? WarrantyExpiry { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Available"; // Available, In Use, Maintenance, Disposed

    public string? PhotoPath { get; set; }
}

public class InventoryTransaction : BaseEntity
{
    public int InventoryItemId { get; set; }

    [ForeignKey("InventoryItemId")]
    public InventoryItem InventoryItem { get; set; } = null!;

    [MaxLength(20)]
    public string TransactionType { get; set; } = "In"; // In, Out, Adjustment

    public int Quantity { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    [MaxLength(100)]
    public string? Reason { get; set; }; // Session use, Purchase, Damage, Return

    public int? BeneficiaryId { get; set; }

    public int? EmployeeId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
