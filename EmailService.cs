using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotifyApp.Models;


public class EmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;
/*     private const long MaxEmailAttachmentSize = 25 * 1024 * 1024;
 */
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
    public async Task SendEmailWithAttachmentsAsync(
        string to,
        string subject,
        string body,
        IEnumerable<string> filePaths)
    {
        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
            };

            using var mail = new MailMessage(_settings.UserName, to, subject, body)
            {
                IsBodyHtml = false
            };

            foreach (var path in filePaths)
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException("Файл не найден", path);

                var fileInfo = new FileInfo(path);

                /* if (fileInfo.Length > MaxEmailAttachmentSize)
                    throw new InvalidOperationException(
                        $"Файл {fileInfo.Name} превышает лимит 25MB"); */

                mail.Attachments.Add(new Attachment(path));
            }

            await client.SendMailAsync(mail);

            _logger.LogInformation("Email с вложениями отправлен");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка отправки Email с вложениями");
            throw;
        }
    }
}
