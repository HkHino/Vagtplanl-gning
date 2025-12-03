using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Authentication;

public class JwtHelper
{
    private readonly IConfiguration _configuration;

    public JwtHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static UserPrincipal GetUser(ClaimsPrincipal claims)
    {
        var user = new UserPrincipal
        {
            Id = Convert.ToInt32(claims.FindFirst(ClaimTypes.NameIdentifier)!.Value),
            FirstName = claims.FindFirst(ClaimTypes.GivenName)!.Value,
            Role = claims.FindFirst(ClaimTypes.Role)!.Value
        };
        return user;
    }

    // TODO: Create user or use the employee?
    public string GenerateToken(Employee user)
    {
        // Json Stringify user
        Console.WriteLine($"Employee ID: {user.EmployeeId}");
        Console.WriteLine($"First Name: {user.FirstName}");
        Console.WriteLine($"Last Name: {user.LastName}");
        Console.WriteLine($"Email: {user.Email}");
        
        var authClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.EmployeeId.ToString()!),
            new(ClaimTypes.GivenName, user.FirstName!),
            new(ClaimTypes.Surname, user.LastName!),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Role, "Admin"),
            // Either use an enum in Employee, or insert another model into the function containing this
            // new(ClaimTypes.Role, user.Role.ToString()),
        };

        var theKey = _configuration["Jwt:Key"] ?? "ZNztR3p+MCOCLtOQe5yTJNJHC1JkiqNfLs6vhaNVzAw=";
        Console.WriteLine($"Key: {theKey}");
        Console.WriteLine($"Claims created: {string.Join(", ", authClaims)}");
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(theKey));
        Console.WriteLine($"Key: {authSigningKey.Key}");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddDays(30),
            Issuer = "Vagtplan.dk",
            Audience = "Vagtplan.dk",
            SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(authClaims)
        };
        Console.WriteLine($"Token created: {tokenDescriptor}");
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}