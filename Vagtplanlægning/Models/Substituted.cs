using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vagtplanlægning.Models;

[Table("Substituteds")]
public class Substituted
{
    [Key]
    [Column("substitutedId")]
    public int SubstitutedId { get; set; }

    [Column("employeeId")]
    public int EmployeeId { get; set; }

    [Column("hasSubstituted")]
    public bool HasSubstituted { get; set; }

    public Employee? Employee { get; set; }
}
