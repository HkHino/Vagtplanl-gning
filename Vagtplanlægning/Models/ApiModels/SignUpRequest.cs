namespace Vagtplanlægning.Models.ApiModels;

public class SignUpRequest
{
// User Info
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

// Employee Info
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int? ExperienceLevel { get; set; }
}