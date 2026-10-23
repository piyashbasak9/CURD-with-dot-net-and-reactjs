
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TaskManager.Application.Interfaces;

namespace TaskManager.Infrastructure.Email;

public class GmailEmailService : IEmailService
{
    private readonly GmailSmtpOptions _options;

    public GmailEmailService(
        IOptions<GmailSmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                _options.SenderName,
                _options.SenderEmail));

        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            _options.Host,
            _options.Port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await client.AuthenticateAsync(
            _options.SenderEmail,
            _options.AppPassword,
            cancellationToken);

        await client.SendAsync(message, cancellationToken);

        await client.DisconnectAsync(
            true,
            cancellationToken);
    }
}
