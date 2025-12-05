namespace Vagtplanlægning.DTOs;

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
    public int? EmployeeId { get; set; }
    public EmployeeDto? Employee { get; set; }
}