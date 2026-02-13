using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotifyApp.Models;


public class EmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
            };

            var mail = new MailMessage(_settings.UserName, to, subject, body) { IsBodyHtml = true };
            await client.SendMailAsync(mail);
            _logger.LogInformation($"Email sent to {to} ({subject})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Email");
            throw;
        }
    }
}
