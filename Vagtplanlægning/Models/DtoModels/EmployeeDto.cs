namespace Vagtplanlægning.DTOs
{
    /// <summary>
    /// Read-only DTO used when returning employee data to clients.
    /// Mirrors the main fields of the <see cref="Vagtplanlægning.Models.Employee"/> entity.
    /// </summary>
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public int ExperienceLevel { get; set; }
    }
}
