using System.Net;
using AllocatrApi.Configuration;
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

    public EmailService(IOptions<EmailSettings> settings,
        ILogger<EmailService> logger, IWebHostEnvironment environment)
    {
        _settings = settings.Value;
        _logger = logger;
        _environment = environment;
    }

    public async Task SendAsync(string toEmail, string subject,
        string htmlBody, string? textBody = null)
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
        if (brandedHtml.Contains($"cid:{EmailLayout.LogoContentId}", StringComparison.OrdinalIgnoreCase))
        {
            var path = Path.Combine(_environment.ContentRootPath,
                "Assets", "Email", "allocatr-logo.png");
            if (!File.Exists(path))
                throw new InvalidOperationException("The email logo is missing. Include Assets/Email/allocatr-logo.png in the published backend.");
            var logo = bodyBuilder.LinkedResources.Add(path);
            logo.ContentId = EmailLayout.LogoContentId;
            logo.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
        }
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            client.CheckCertificateRevocation = _settings.CheckCertificateRevocation;
            var socketOptions = _settings.Port == 465 || _settings.Port == 2465
                ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("Email sent successfully to {Email}.", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not send email to {Email}. Subject: {Subject}", toEmail, subject);
            throw new InvalidOperationException("The email could not be sent at this time.", ex);
        }
    }

    public async Task SendEmailVerificationAsync(string email,
        string fullName, string verificationUrl)
    {
        var safeName = WebUtility.HtmlEncode(fullName);
        var safeVerificationUrl = WebUtility.HtmlEncode(verificationUrl);
        var subject = "Verify your Allocatr email address";
        var action = EmailLayout.ActionLink("Verify email", verificationUrl);
        var htmlBody = $"""
            <h1 style="margin:0;font-family:{EmailLayout.FontFamily};font-size:24px;font-weight:600;line-height:32px;letter-spacing:-0.5px;color:#333333;">Verify your email address</h1>
            <p style="margin:20px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#555d52;">Hi {safeName},</p>
            <p style="margin:10px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#555d52;">Confirm that this email address belongs to you to continue to your Allocatr workspace.</p>
            {action}
            <p style="margin:0;font-family:{EmailLayout.FontFamily};font-size:12px;line-height:22px;color:#697066;">If you did not request this email, you can safely ignore it.</p>
            <div style="margin-top:24px;padding-top:18px;border-top:1px solid #e6e9e1;">
                <p style="margin:0;font-family:{EmailLayout.FontFamily};font-size:11px;line-height:20px;color:#82877e;">If the button does not work, copy this link into your browser:</p>
                <p style="margin:6px 0 0;font-family:{EmailLayout.FontFamily};font-size:11px;line-height:20px;word-break:break-all;color:#697066;">{safeVerificationUrl}</p>
            </div>
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
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
        text = WebUtility.HtmlDecode(text);
        return System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
    }
}
