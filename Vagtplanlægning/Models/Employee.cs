using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Column("employeeId")] public int EmployeeId { get; set; }
        [Column("firstName")] public string FirstName { get; set; } = "";
        [Column("lastName")] public string LastName { get; set; } = "";
        [Column("address")] public string Address { get; set; } = "";
        [Column("phone")] public string Phone { get; set; } = "";
        [Column("email")] public string Email { get; set; } = "";
        [Column("experienceLevel")] public int ExperienceLevel { get; set; } = 1;
    }
}

