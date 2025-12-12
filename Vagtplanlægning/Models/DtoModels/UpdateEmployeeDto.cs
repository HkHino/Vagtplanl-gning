namespace Vagtplanlægning.DTOs
{
    /// <summary>
    /// DTO used when updating an existing employee.
    ///
    /// Typically used together with the employee id in the route.
    /// </summary>
    public class UpdateEmployeeDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public int ExperienceLevel { get; set; }
    }
}
