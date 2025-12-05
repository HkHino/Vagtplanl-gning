namespace Vagtplanlægning.Authentication;

public class UserPrincipal
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Role { get; set; } = null!;
}