using System.Net;
using System.Text.RegularExpressions;

namespace AllocatrApi.Services;

/// <summary>Shared Allocatr email typography, branding and actions.</summary>
public static class EmailLayout
{
    public const string LogoContentId = "allocatr-email-logo";
    public const int LogoWidth = 100;
    public const int LogoHeight = 30;
    public const string FontFamily = "Poppins, 'Segoe UI', Arial, Helvetica, sans-serif";
    private const string Marker = "<!-- allocatr-email-layout:v1 -->";

    /// <param name="contentHtml">Message-specific HTML with dynamic text HTML-encoded by its caller.</param>
    public static string Render(string subject, string contentHtml)
    {
        ArgumentNullException.ThrowIfNull(contentHtml);
        if (contentHtml.Contains(Marker, StringComparison.Ordinal)) return contentHtml;
        var body = Regex.Match(contentHtml, @"<body\b[^>]*>([\s\S]*?)</body\s*>",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        var content = body.Success ? body.Groups[1].Value : contentHtml;
        var title = WebUtility.HtmlEncode(subject);
        var year = DateTime.UtcNow.Year;
        return $$"""
            <!DOCTYPE html>
            {{Marker}}
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{{title}}</title>
                <!--[if !mso]><!-->
                <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600&amp;display=swap">
                <!--<![endif]-->
                <style>
                    @media only screen and (max-width:480px) {
                        .allocatr-email-outer { padding:20px 12px !important; }
                        .allocatr-email-header { padding:20px 24px !important; }
                        .allocatr-email-content { padding:28px 24px !important; }
                    }
                </style>
                <!--[if mso]>
                <style>body,table,td,p,h1,h2,a { font-family:Arial,Helvetica,sans-serif !important; }</style>
                <![endif]-->
            </head>
            <body style="margin:0;padding:0;background-color:#f3f4f1;font-family:{{FontFamily}};color:#333333;-webkit-text-size-adjust:100%;">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" bgcolor="#f3f4f1" style="background-color:#f3f4f1;">
                    <tr><td align="center" class="allocatr-email-outer" style="padding:40px 20px;">
                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" bgcolor="#ffffff" style="max-width:560px;background-color:#ffffff;border:1px solid #e1e4dc;border-radius:14px;">
                            <tr>
                                <td bgcolor="#333333" class="allocatr-email-header" style="padding:22px 32px;background-color:#333333;background-image:linear-gradient(#333333,#333333);border-radius:13px 13px 0 0;">
                                    <img src="cid:{{LogoContentId}}" alt="Allocatr" width="{{LogoWidth}}" height="{{LogoHeight}}" style="display:block;width:{{LogoWidth}}px;height:{{LogoHeight}}px;max-width:100%;border:0;outline:none;text-decoration:none;">
                                </td>
                            </tr>
                            <tr><td class="allocatr-email-content" style="padding:32px;font-family:{{FontFamily}};font-size:14px;font-weight:400;line-height:25px;color:#333333;">
                                {{content}}
                            </td></tr>
                        </table>
                        <p style="margin:22px 0 0;font-family:{{FontFamily}};font-size:11px;font-weight:500;line-height:20px;color:#697066;">Work, better allocated.</p>
                        <p style="margin:4px 0 0;font-family:{{FontFamily}};font-size:10px;line-height:18px;color:#82877e;">&copy; {{year}} Allocatr</p>
                    </td></tr>
                </table>
            </body>
            </html>
            """;
    }

    /// <summary>A compact charcoal action button with lime text for current and future templates.</summary>
    public static string ActionLink(string label, string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var target)
            || (target.Scheme != Uri.UriSchemeHttps && target.Scheme != Uri.UriSchemeHttp))
            throw new ArgumentException("Email action links must be absolute HTTP or HTTPS URLs.", nameof(url));
        var safeLabel = WebUtility.HtmlEncode(label);
        var safeUrl = WebUtility.HtmlEncode(url);
        return $"""
            <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin:24px 0;">
                <tr><td bgcolor="#333333" style="border-radius:7px;background-color:#333333;">
                    <a href="{safeUrl}" style="display:inline-block;padding:10px 16px;border:1px solid #333333;border-radius:7px;background-color:#333333;background-image:linear-gradient(#333333,#333333);font-family:{FontFamily};font-size:12px;font-weight:600;line-height:18px;color:#DDDB00;text-decoration:none;mso-padding-alt:0;text-align:center;">{safeLabel}</a>
                </td></tr>
            </table>
            """;
    }
}
