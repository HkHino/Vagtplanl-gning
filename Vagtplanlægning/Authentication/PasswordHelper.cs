using DevOne.Security.Cryptography.BCrypt;

namespace Vagtplanlægning.Authentication;

public static class PasswordHelper
{
    // Message: Password in string form? Salt: Salt uden pepper
    // -- 1st: GenerateSalt();
    // -- 2nd: HashPassword
    
    /*
     var hashedPassword = PasswordHelper.HashPassword(request.Password, user.Salt);
        if (hashedPassword != user.Hash)
        {
            return BadRequest("Incorrect password");
        }
     
     
     
     */
    
    public static string HashPassword(string message, string salt)
    {
        var hashedPassword = BCryptHelper.HashPassword(message, salt);
        return hashedPassword;
    }
    
    public static string GenerateSalt()
    {
        const int workFactor = 5;
        var salt = BCryptHelper.GenerateSalt(workFactor);
        return salt;
    }
    
}