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
            Username = claims.FindFirst(ClaimTypes.Name)!.Value,
            Role = claims.FindFirst(ClaimTypes.Role)!.Value
        };
        return user;
    }

    // TODO: Create user or use the employee?
    public string GenerateToken(User user)
    {
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
            
        };
        
        var jwtKey = _configuration["Key"];
        if(string.IsNullOrEmpty(jwtKey)) throw new Exception("JWT Key not set");
        
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddDays(30),
            Issuer = "Vagtplan.dk",
            Audience = "Vagtplan.dk",
            SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(authClaims)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}