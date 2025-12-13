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
    private readonly ILogger<IEmailManager> _logger;

    public EmailService(IConfiguration configuration,
        ILogger<IEmailManager> logger
        )
    {
        _configuration = configuration;
        _logger = logger;
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
            _logger.LogInformation("Send email successful from: ", mail);
            return true;
        }
        catch (Exception)
        {
            _logger.LogError("Failed sending email from: ", mail);
            return false;
        }
    }
}