namespace Vagtplanlægning.Authentication;

public class UserPrincipal
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Role { get; set; } = null!;
}