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

    public EmailService(
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger
    )
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /* --------------------------------------------------------
     * SEND
     * -------------------------------------------------------- */

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? textBody = null
    )
    {
        ValidateConfiguration();

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException(
                "Recipient email is required."
            );
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException(
                "Email subject is required."
            );
        }

        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                _settings.FromName,
                _settings.FromEmail
            )
        );

        message.To.Add(
            MailboxAddress.Parse(toEmail)
        );

        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = textBody ?? StripHtml(htmlBody),
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        // try
        // {
        //     await client.ConnectAsync(
        //         _settings.Host,
        //         _settings.Port,
        //         SecureSocketOptions.Auto
        //     );

        //     if (!string.IsNullOrWhiteSpace(_settings.Username))
        //     {
        //         await client.AuthenticateAsync(
        //             _settings.Username,
        //             _settings.Password
        //         );
        //     }

        //     await client.SendAsync(message);

        //     await client.DisconnectAsync(true);

        //     _logger.LogInformation(
        //         "Email sent successfully to {Email}. Subject: {Subject}",
        //         toEmail,
        //         subject
        //     );
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(
        //         ex,
        //         "Could not send email to {Email}. Subject: {Subject}",
        //         toEmail,
        //         subject
        //     );

        //     throw new InvalidOperationException(
        //         "The email could not be sent at this time.",
        //         ex
        //     );
        // }

        try
        {
            client.CheckCertificateRevocation =
                _settings.CheckCertificateRevocation;

            var socketOptions =
                _settings.Port == 465 ||
                _settings.Port == 2465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                socketOptions
            );

            await client.AuthenticateAsync(
                _settings.Username,
                _settings.Password
            );

            await client.SendAsync(message);

            await client.DisconnectAsync(true);

            _logger.LogInformation(
                "Email sent successfully to {Email}.",
                toEmail
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Could not send email to {Email}. Subject: {Subject}",
                toEmail,
                subject
            );

            throw new InvalidOperationException(
                "The email could not be sent at this time.",
                ex
            );
        }
    }

    /* --------------------------------------------------------
     * EMAIL VERIFICATION
     * -------------------------------------------------------- */

    public async Task SendEmailVerificationAsync(
        string email,
        string fullName,
        string verificationUrl
    )
    {
        var safeName =
            WebUtility.HtmlEncode(fullName);

        var safeVerificationUrl =
            WebUtility.HtmlEncode(verificationUrl);

        var subject =
            "Verify your Allocatr email address";

        var htmlBody = $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta
                    name="viewport"
                    content="width=device-width, initial-scale=1.0"
                >
                <title>Verify your email</title>
            </head>

            <body
                style="
                    margin: 0;
                    padding: 0;
                    background: #f5f5f5;
                    font-family: Arial, Helvetica, sans-serif;
                    color: #18181b;
                "
            >
                <table
                    role="presentation"
                    width="100%"
                    cellspacing="0"
                    cellpadding="0"
                    border="0"
                    style="background: #f5f5f5;"
                >
                    <tr>
                        <td
                            align="center"
                            style="padding: 40px 20px;"
                        >
                            <table
                                role="presentation"
                                width="100%"
                                cellspacing="0"
                                cellpadding="0"
                                border="0"
                                style="
                                    max-width: 560px;
                                    background: #ffffff;
                                    border: 1px solid #e4e4e7;
                                    border-radius: 16px;
                                "
                            >
                                <tr>
                                    <td
                                        style="
                                            padding: 36px;
                                        "
                                    >
                                        <p
                                            style="
                                                margin: 0 0 24px;
                                                font-size: 20px;
                                                font-weight: 800;
                                                letter-spacing: -0.5px;
                                            "
                                        >
                                            Allocatr
                                        </p>

                                        <h1
                                            style="
                                                margin: 0;
                                                font-size: 26px;
                                                line-height: 1.25;
                                                letter-spacing: -0.7px;
                                            "
                                        >
                                            Verify your email address
                                        </h1>

                                        <p
                                            style="
                                                margin: 20px 0 0;
                                                font-size: 15px;
                                                line-height: 1.7;
                                                color: #52525b;
                                            "
                                        >
                                            Hi {safeName},
                                        </p>

                                        <p
                                            style="
                                                margin: 12px 0 0;
                                                font-size: 15px;
                                                line-height: 1.7;
                                                color: #52525b;
                                            "
                                        >
                                            Confirm that this email address
                                            belongs to you by clicking the
                                            button below.
                                        </p>

                                        <table
                                            role="presentation"
                                            cellspacing="0"
                                            cellpadding="0"
                                            border="0"
                                            style="
                                                margin-top: 28px;
                                            "
                                        >
                                            <tr>
                                                <td
                                                    style="
                                                        border-radius: 8px;
                                                        background: #18181b;
                                                    "
                                                >
                                                    <a
                                                        href="{safeVerificationUrl}"
                                                        style="
                                                            display: inline-block;
                                                            padding: 13px 20px;
                                                            color: #ffffff;
                                                            text-decoration: none;
                                                            font-size: 14px;
                                                            font-weight: 700;
                                                        "
                                                    >
                                                        Verify email
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>

                                        <p
                                            style="
                                                margin: 28px 0 0;
                                                font-size: 13px;
                                                line-height: 1.6;
                                                color: #71717a;
                                            "
                                        >
                                            If you did not request this email,
                                            you can safely ignore it.
                                        </p>

                                        <div
                                            style="
                                                margin-top: 28px;
                                                padding-top: 20px;
                                                border-top: 1px solid #e4e4e7;
                                            "
                                        >
                                            <p
                                                style="
                                                    margin: 0;
                                                    font-size: 11px;
                                                    line-height: 1.6;
                                                    color: #a1a1aa;
                                                "
                                            >
                                                If the button does not work,
                                                copy and paste this link into
                                                your browser:
                                            </p>

                                            <p
                                                style="
                                                    margin: 8px 0 0;
                                                    font-size: 11px;
                                                    line-height: 1.6;
                                                    word-break: break-all;
                                                    color: #71717a;
                                                "
                                            >
                                                {safeVerificationUrl}
                                            </p>
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <p
                                style="
                                    margin: 18px 0 0;
                                    font-size: 11px;
                                    color: #a1a1aa;
                                "
                            >
                                Allocatr
                            </p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;

        var textBody = $"""
            Hi {fullName},

            Verify your Allocatr email address by opening the link below:

            {verificationUrl}

            If you did not request this email, you can safely ignore it.

            Allocatr
            """;

        await SendAsync(
            email,
            subject,
            htmlBody,
            textBody
        );
    }

    /* --------------------------------------------------------
     * VALIDATION
     * -------------------------------------------------------- */

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            throw new InvalidOperationException(
                "Email SMTP host has not been configured."
            );
        }

        if (_settings.Port <= 0)
        {
            throw new InvalidOperationException(
                "Email SMTP port has not been configured."
            );
        }

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException(
                "Email sender address has not been configured."
            );
        }
    }

    /* --------------------------------------------------------
     * HELPERS
     * -------------------------------------------------------- */

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var text = System.Text.RegularExpressions.Regex.Replace(
            html,
            "<[^>]+>",
            " "
        );

        text = WebUtility.HtmlDecode(text);

        return System.Text.RegularExpressions.Regex.Replace(
            text,
            @"\s+",
            " "
        ).Trim();
    }
}