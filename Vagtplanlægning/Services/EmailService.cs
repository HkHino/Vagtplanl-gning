using System.Net;
using System.Net.Mail;

namespace Vagtplanlægning.Services;


// TODO: Purely copy pasted, to be worked on if time, else just delete it
public interface IEmailManager
{
    Task<bool> SendEmail(string email, string subject, string message);
}

public class EmailService : IEmailManager
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> SendEmail(string email, string subject, string message)
    {
        var mail = _configuration["Email:Login"];
        var pw = _configuration["Email:Password"];
        var client = new SmtpClient("smtp-mail.outlook.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(mail, pw)
        };
        try
        {
            await client.SendMailAsync(
                new MailMessage(from: mail,
                    to: email,
                    subject,
                    message));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}