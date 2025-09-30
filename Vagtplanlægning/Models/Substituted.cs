using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models
{
    [Table("Substituteds")]
    public class Substituted
    {
        [Column("substitutedId")] public int SubstitutedId { get; set; }
        [Column("employeeId")] public int EmployeeId { get; set; }
        [Column("hasSubstituted")] public bool HasSubstituted { get; set; }
    }
}
