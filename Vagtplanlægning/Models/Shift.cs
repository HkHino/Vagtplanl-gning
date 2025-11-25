using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models;

/// <summary>
/// Represents a single work shift in the ListOfShift table.
/// Each row ties an employee, bicycle and route together on a specific date,
/// with optional start/end times and a calculated total number of hours.
/// </summary>
[Table("ListOfShift")]
public class Shift
{
    [Key]
    [Column("shiftId")]
    public int ShiftId { get; set; }

    /// <summary>
    /// Calendar date of the shift (maps to dateOfShift).
    /// </summary>
    [Column("dateOfShift")]
    public DateTime DateOfShift { get; set; }

    [Column("employeeId")]
    public int EmployeeId { get; set; }

    [Column("bicycleId")]
    public int BicycleId { get; set; }

    /// <summary>
    /// Foreign key to Substituteds.substitutedId.
    /// The database column is NOT NULL, so we keep this as non-nullable int.
    /// </summary>
    [Column("substitutedId")]
    public int SubstitutedId { get; set; }

    [Column("routeId")]
    public int RouteId { get; set; }

    /// <summary>
    /// Optional start time for the shift.
    /// </summary>
    [Column("startTime")]
    public TimeSpan? StartTime { get; set; }

    /// <summary>
    /// Optional end time for the shift.
    /// </summary>
    [Column("endTime")]
    public TimeSpan? EndTime { get; set; }

    /// <summary>
    /// Total hours for the shift. This is calculated in the database
    /// via triggers when both startTime and endTime are present.
    /// </summary>
    [Column("totalHours")]
    public decimal? TotalHours { get; set; }

    // Navigation properties
    public Employee? Employee { get; set; }
    public Bicycle? Bicycle { get; set; }
    public Substituted? Substituted { get; set; }
    public RouteEntity? Routes { get; set; }
}
