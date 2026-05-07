using HabitHub.API.Models.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HabitHub.API.Services;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string toName, string subject, string message);
}

public class EmailSender : IEmailSender
{
    private readonly MailSettings _mailSettings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<MailSettings> mailSettings, ILogger<EmailSender> logger)
    {
        _mailSettings = mailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string message)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail));
        email.To.Add(new MailboxAddress(toName, toEmail));
        email.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            TextBody = message,
            HtmlBody = $"<html><body><p>{message.Replace(Environment.NewLine, "<br/>")}</p></body></html>"
        };
        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        try
        {
            await smtp.ConnectAsync(_mailSettings.Server, _mailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);

            await smtp.SendAsync(email);

            _logger.LogInformation("Email sent to {Email} with subject '{Subject}'", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw; 
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }
}