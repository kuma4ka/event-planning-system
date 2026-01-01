using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EventPlanning.Infrastructure.Services;

public class SmtpEmailService(
    IOptions<EmailSettings> emailSettings,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly EmailSettings _settings = emailSettings.Value;

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(_settings.Server, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", to);
            // Suppress exception to prevent flow interruption; SMTP failure shouldn't block user action.
        }
    }
}
