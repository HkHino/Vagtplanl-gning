using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models;

/// <summary>
/// Courier / employee entity backed by Employees table.
/// </summary>
[Table("Employees")]
public class Employee
{
    [Key]
    [Column("employeeId")]
    public int EmployeeId { get; set; }

    [Column("firstName")]
    [MaxLength(255)]
    public string FirstName { get; set; } = string.Empty;

    [Column("lastName")]
    [MaxLength(255)]
    public string LastName { get; set; } = string.Empty;

    [Column("address")]
    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    [Column("phone")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Experience level 1..n (column: experienceLevel).
    /// </summary>
    [Column("experienceLevel")]
    public int ExperienceLevel { get; set; } = 1;

    // Navigation properties
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    public ICollection<Substituted> SubstitutionRecords { get; set; } = new List<Substituted>();
}
