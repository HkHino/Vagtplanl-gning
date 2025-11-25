using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models;

/// <summary>
/// Represents a bicycle used by couriers.
/// Backed by the Bicycles table.
/// </summary>
[Table("Bicycles")]
public class Bicycle
{
    /// <summary>
    /// Database identity column (id).
    /// We keep the CLR name BicycleId to avoid changing the rest of the code.
    /// </summary>
    [Key]
    [Column("id")]
    public int BicycleId { get; set; }

    /// <summary>
    /// Business-facing bicycle number (bicycleNumber).
    /// </summary>
    [Column("bicycleNumber")]
    public int BicycleNumber { get; set; }

    /// <summary>
    /// Whether this bicycle is currently in operation.
    /// </summary>
    [Column("inOperate")]
    public bool InOperate { get; set; }

    // Navigation – all shifts that used this bicycle.
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
