using System;
using System.ComponentModel.DataAnnotations;

namespace Vagtplanlægning.DTOs;

/// <summary>
/// Payload for creating a new shift.
/// </summary>
public class CreateShiftDto
{
    /// <summary>
    /// Date of the shift (maps to ListOfShift.dateOfShift).
    /// Only the date part is used – time-of-day is ignored.
    /// </summary>
    [Required]
    public DateTime DateOfShift { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int BicycleId { get; set; }

    [Required]
    public int RouteId { get; set; }

    /// <summary>
    /// Foreign key to Substituteds.substitutedId.
    /// In the current database this is NOT NULL,
    /// so the client must choose one of the existing substitute records.
    /// </summary>
    [Required]
    public int SubstitutedId { get; set; }
}
