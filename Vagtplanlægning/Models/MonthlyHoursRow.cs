using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vagtplanlægning.Models
{
    [Keyless]
    public class MonthlyHoursRow
    {
        [Column("employeeId")] public int EmployeeId { get; set; }
        [Column("firstName")] public string FirstName { get; set; } = "";
        [Column("lastName")] public string LastName { get; set; } = "";
        [Column("year")] public int Year { get; set; }
        [Column("month")] public int Month { get; set; }
        [Column("totalMonthlyHours")] public decimal TotalMonthlyHours { get; set; }
        [Column("hasSubstituted")] public bool HasSubstituted { get; set; }
    }
}
