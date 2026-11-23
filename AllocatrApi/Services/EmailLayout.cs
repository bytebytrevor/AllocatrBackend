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
                <meta name="color-scheme" content="light dark">
                <meta name="supported-color-schemes" content="light dark">
                <title>{{title}}</title>
                <!--[if !mso]><!-->
                <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600&amp;display=swap">
                <!--<![endif]-->
                <style>
                    :root { color-scheme:light dark; supported-color-schemes:light dark; }
                    .allocatr-email-action, .allocatr-email-action span { color:#ffffff !important; -webkit-text-fill-color:#ffffff !important; }
                    @media (prefers-color-scheme:dark) {
                        .allocatr-email-canvas { background-color:#242424 !important; }
                        .allocatr-email-card, .allocatr-email-content { background-color:#333333 !important; color:#f3f4f1 !important; }
                        .allocatr-email-card, .allocatr-email-rule { border-color:#484848 !important; }
                        .allocatr-email-heading { color:#f3f4f1 !important; }
                        .allocatr-email-text { color:#d2d6ce !important; }
                        .allocatr-email-muted { color:#b6bbb2 !important; }
                        .allocatr-email-quiet { color:#a1a69d !important; }
                        .allocatr-email-panel { background-color:#3b3b3b !important; border-color:#484848 !important; }
                        .allocatr-email-action { border-color:#242424 !important; background-color:#242424 !important; background-image:linear-gradient(#242424,#242424) !important; }
                        .allocatr-email-action, .allocatr-email-action span { color:#ffffff !important; -webkit-text-fill-color:#ffffff !important; }
                    }
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
            <body class="allocatr-email-canvas" style="margin:0;padding:0;background-color:#f3f4f1;font-family:{{FontFamily}};color:#333333;-webkit-text-size-adjust:100%;">
                <table role="presentation" class="allocatr-email-canvas" width="100%" cellspacing="0" cellpadding="0" border="0" bgcolor="#f3f4f1" style="background-color:#f3f4f1;">
                    <tr><td align="center" class="allocatr-email-outer" style="padding:40px 20px;">
                        <table role="presentation" class="allocatr-email-card" width="100%" cellspacing="0" cellpadding="0" border="0" bgcolor="#ffffff" style="max-width:560px;background-color:#ffffff;border:1px solid #e1e4dc;border-radius:0;">
                            <tr>
                                <td bgcolor="#333333" class="allocatr-email-header" style="padding:22px 32px;background-color:#333333;background-image:linear-gradient(#333333,#333333);border-radius:0;">
                                    <img src="cid:{{LogoContentId}}" alt="Allocatr" width="{{LogoWidth}}" height="{{LogoHeight}}" style="display:block;width:{{LogoWidth}}px;height:{{LogoHeight}}px;max-width:100%;border:0;outline:none;text-decoration:none;">
                                </td>
                            </tr>
                            <tr><td bgcolor="#ffffff" class="allocatr-email-content" style="padding:32px;background-color:#ffffff;font-family:{{FontFamily}};font-size:14px;font-weight:400;line-height:25px;color:#333333;">
                                {{content}}
                            </td></tr>
                        </table>
                        <p class="allocatr-email-muted" style="margin:22px 0 0;font-family:{{FontFamily}};font-size:11px;font-weight:500;line-height:20px;color:#697066;">Work, better allocated.</p>
                        <p class="allocatr-email-quiet" style="margin:4px 0 0;font-family:{{FontFamily}};font-size:10px;line-height:18px;color:#82877e;">&copy; {{year}} Allocatr</p>
                    </td></tr>
                </table>
            </body>
            </html>
            """;
    }

    /// <summary>A compact rounded charcoal action button with white text.</summary>
    public static string ActionLink(string label, string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var target)
            || (target.Scheme != Uri.UriSchemeHttps && target.Scheme != Uri.UriSchemeHttp))
            throw new ArgumentException("Email action links must be absolute HTTP or HTTPS URLs.", nameof(url));
        var safeLabel = WebUtility.HtmlEncode(label);
        var safeUrl = WebUtility.HtmlEncode(url);
        return $"""
            <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin:24px 0;">
                <tr><td bgcolor="#333333" style="border-radius:8px;background-color:#333333;">
                    <a class="allocatr-email-action" href="{safeUrl}" style="display:inline-block;padding:10px 16px;border:1px solid #333333;border-radius:8px;background-color:#333333;background-image:linear-gradient(#333333,#333333);font-family:{FontFamily};font-size:12px;font-weight:600;line-height:18px;color:#ffffff;-webkit-text-fill-color:#ffffff;text-decoration:none;mso-padding-alt:0;text-align:center;"><span style="color:#ffffff;-webkit-text-fill-color:#ffffff;">{safeLabel}</span></a>
                </td></tr>
            </table>
            """;
    }
}
