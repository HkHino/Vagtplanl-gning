namespace Vagtplanlægning.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public UserRole Role { get; set; }
    public string Hash { get; set; }
    
    // Has one employee connected to it
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;
}

public enum UserRole
{
    Admin,
    Employee
}