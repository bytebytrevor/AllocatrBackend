using System.Net;
using AllocatrApi.Configuration;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AllocatrApi.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _environment;

    public EmailService(
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger,
        IWebHostEnvironment environment
    )
    {
        _settings = settings.Value;
        _logger = logger;
        _environment = environment;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? textBody = null
    )
    {
        ValidateConfiguration();
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Recipient email is required.");
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Email subject is required.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        var brandedHtml = EmailLayout.Render(subject, htmlBody);
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = brandedHtml,
            TextBody = textBody ?? StripHtml(htmlBody),
        };
        if (
            brandedHtml.Contains(
                $"cid:{EmailLayout.LogoContentId}",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            var path = Path.Combine(
                _environment.ContentRootPath,
                "Assets",
                "Email",
                "allocatr-logo.png"
            );
            if (!File.Exists(path))
                throw new InvalidOperationException(
                    "The email logo is missing. Include Assets/Email/allocatr-logo.png in the published backend."
                );
            var logo = bodyBuilder.LinkedResources.Add(path);
            logo.ContentId = EmailLayout.LogoContentId;
            logo.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
        }
        if (
            brandedHtml.Contains(
                $"cid:{EmailLayout.ProjectIconContentId}",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            var iconPath = Path.Combine(
                _environment.ContentRootPath,
                "Assets",
                "Email",
                "project-folder.png"
            );
            if (!File.Exists(iconPath))
                throw new InvalidOperationException(
                    "The email project icon is missing. Include Assets/Email/project-folder.png in the published backend."
                );
            var icon = bodyBuilder.LinkedResources.Add(iconPath);
            icon.ContentId = EmailLayout.ProjectIconContentId;
            icon.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
            var darkIconPath = Path.Combine(
                _environment.ContentRootPath,
                "Assets",
                "Email",
                "project-folder-dark.png"
            );
            if (!File.Exists(darkIconPath))
                throw new InvalidOperationException(
                    "The dark email project icon is missing. Include Assets/Email/project-folder-dark.png in the published backend."
                );
            var darkIcon = bodyBuilder.LinkedResources.Add(darkIconPath);
            darkIcon.ContentId = EmailLayout.DarkProjectIconContentId;
            darkIcon.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
        }
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            client.CheckCertificateRevocation = _settings.CheckCertificateRevocation;
            var socketOptions =
                _settings.Port == 465 || _settings.Port == 2465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;
            await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(new SendRequest(message));
            await client.DisconnectAsync(true);
            _logger.LogInformation("Email sent successfully to {Email}.", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Could not send email to {Email}. Subject: {Subject}",
                toEmail,
                subject
            );
            throw new InvalidOperationException("The email could not be sent at this time.", ex);
        }
    }

    public async Task SendEmailVerificationAsync(
        string email,
        string fullName,
        string verificationUrl
    )
    {
        var safeName = WebUtility.HtmlEncode(fullName);
        var eyebrow = EmailLayout.Eyebrow("Account verification");
        var fallback = EmailLayout.FallbackLink(verificationUrl);
        var subject = "Verify your Allocatr email address";
        var action = EmailLayout.ActionLink("Verify email", verificationUrl);
        var htmlBody = $"""
            {eyebrow}
            <h1 class="allocatr-email-heading" style="margin:0;font-family:{EmailLayout.FontFamily};font-size:26px;font-weight:600;line-height:35px;letter-spacing:-0.5px;color:#303638;">Verify your email address</h1>
            <p class="allocatr-email-text" style="margin:20px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#47575C;">Hi {safeName},</p>
            <p class="allocatr-email-text" style="margin:10px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#47575C;">Confirm that this email address belongs to you to continue to your Allocatr workspace.</p>
            {action}
            <p class="allocatr-email-muted" style="margin:0;font-family:{EmailLayout.FontFamily};font-size:12px;line-height:22px;color:#667477;">If you did not request this email, you can safely ignore it.</p>
            {fallback}
            """;
        var textBody = $"""
            Hi {fullName},

            Verify your Allocatr email address by opening the link below:

            {verificationUrl}

            If you did not request this email, you can safely ignore it.

            Allocatr
            """;
        await SendAsync(email, subject, htmlBody, textBody);
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
            throw new InvalidOperationException("Email SMTP host has not been configured.");
        if (_settings.Port <= 0)
            throw new InvalidOperationException("Email SMTP port has not been configured.");
        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
            throw new InvalidOperationException("Email sender address has not been configured.");
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
        text = WebUtility.HtmlDecode(text);
        return System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
    }
}
