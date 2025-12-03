using DevOne.Security.Cryptography.BCrypt;

namespace Vagtplanlægning.Authentication;

public static class PasswordHelper
{
    // Message: Password in string form? Salt: Salt uden pepper
    public static string HashPassword(string message, string salt)
    {
        var hashedPassword = BCryptHelper.HashPassword(message, salt);
        return hashedPassword;
    }
    
    public static string GenerateSalt(string password)
    {
        const int workFactor = 5;
        var salt = BCryptHelper.GenerateSalt(workFactor);
        return salt;
    }
    
}