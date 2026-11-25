using System.Net;
using System.Text.RegularExpressions;

namespace AllocatrApi.Services;

/// <summary>Shared, progressively enhanced presentation for all Allocatr emails.</summary>
public static class EmailLayout
{
    public const string LogoContentId = "allocatr-email-logo";
    public const string ProjectIconContentId = "allocatr-email-project-icon";
    public const string DarkProjectIconContentId = "allocatr-email-project-icon-dark";
    public const int LogoWidth = 100;
    public const int LogoHeight = 30;
    public const string FontFamily = "Poppins, 'Segoe UI', Arial, Helvetica, sans-serif";
    public const string Teal = "#315E6C";
    public const string Lime = "#DEDA00";
    public const string Charcoal = "#303638";
    private const string Marker = "<!-- allocatr-email-layout:v2 -->";

    /// <param name="contentHtml">Message-specific HTML; callers must HTML-encode dynamic text.</param>
    public static string Render(string subject, string contentHtml)
    {
        ArgumentNullException.ThrowIfNull(contentHtml);
        // Preserve the original idempotence contract, including v1-wrapped callers.
        if (
            contentHtml.Contains(Marker, StringComparison.Ordinal)
            || contentHtml.Contains("<!-- allocatr-email-layout:v1 -->", StringComparison.Ordinal)
        )
            return contentHtml;
        var body = Regex.Match(
            contentHtml,
            @"<body\b[^>]*>([\s\S]*?)</body\s*>",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1)
        );
        var content = body.Success ? body.Groups[1].Value : contentHtml;
        var title = WebUtility.HtmlEncode(subject);
        var year = DateTime.UtcNow.Year;
        // Explicit dark styles are enhancements; essential layout remains inline.
        var darkRules = DarkRules();
        var outlookRules = Regex.Replace(
            darkRules,
            @"(?m)^([ .][^{]+)\{",
            match =>
                string.Join(
                    ", ",
                    match
                        .Groups[1]
                        .Value.Trim()
                        .Split(',')
                        .SelectMany(selector =>
                            new[]
                            {
                                "[data-ogsc] " + selector.Trim(),
                                "[data-ogsb] " + selector.Trim(),
                            }
                        )
                ) + " {",
            RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1)
        );
        return $$"""
            <!DOCTYPE html>
            {{Marker}}
            <html lang="en" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:w="urn:schemas-microsoft-com:office:word">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <meta name="color-scheme" content="light dark">
                <meta name="supported-color-schemes" content="light dark">
                <title>{{title}}</title>
                <!--[if !mso]><!--><link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600&amp;display=swap"><!--<![endif]-->
                <style>
                    :root { color-scheme:light dark; supported-color-schemes:light dark; }
                    table { border-collapse:separate; border-spacing:0; mso-table-lspace:0pt; mso-table-rspace:0pt; }
                    a[x-apple-data-detectors] { color:inherit !important; text-decoration:none !important; }
                    .allocatr-email-header { background-color:{{Charcoal}} !important; }
                    .allocatr-email-action, .allocatr-email-action span { color:#ffffff !important; -webkit-text-fill-color:#ffffff !important; }
                    .allocatr-email-action:hover { opacity:0.88 !important; }
                    .allocatr-email-link:hover { opacity:0.75 !important; }
                    @media (prefers-color-scheme:dark) {
                        {{darkRules}}
                    }
                    {{outlookRules}}
                    @media only screen and (max-width:480px) {
                        .allocatr-email-outer { padding:16px 10px !important; }
                        .allocatr-email-header { padding:20px 24px !important; }
                        .allocatr-email-content { padding:28px 24px !important; }
                        .allocatr-email-heading { font-size:24px !important; line-height:32px !important; }
                        .allocatr-project-inner { padding:16px !important; }
                        .allocatr-project-icon-cell { width:44px !important; padding-right:12px !important; }
                        .allocatr-project-title { font-size:17px !important; line-height:26px !important; }
                        .allocatr-project-reference-label, .allocatr-project-reference-value { display:block !important; width:auto !important; text-align:left !important; }
                        .allocatr-project-reference-value { padding-top:8px !important; }
                    }
                </style>
                <!--[if mso]><style>body,table,td,p,h1,h2,a { font-family:Arial,Helvetica,sans-serif !important; }</style><![endif]-->
            </head>
            <body class="allocatr-email-canvas" style="margin:0;padding:0;background-color:#F1F3F0;font-family:{{FontFamily}};color:{{Charcoal}};-webkit-text-size-adjust:100%;">
                <table role="presentation" class="allocatr-email-canvas" width="100%" cellspacing="0" cellpadding="0" border="0" bgcolor="#F1F3F0" style="width:100%;background-color:#F1F3F0;">
                    <tr><td align="center" class="allocatr-email-outer" style="padding:40px 20px;">
                        <!--[if mso]><table role="presentation" width="600" cellspacing="0" cellpadding="0" border="0"><tr><td><![endif]-->
                        <table role="presentation" class="allocatr-email-card" width="100%" cellspacing="0" cellpadding="0" border="0" bgcolor="#FBFCF9" style="width:100%;max-width:600px;background-color:#FBFCF9;border:1px solid #D9E1DD;border-radius:0;">
                            <tr><td bgcolor="{{Charcoal}}" class="allocatr-email-header" style="padding:22px 36px;background-color:{{Charcoal}};background-image:linear-gradient({{Charcoal}},{{Charcoal}});">
                                <img src="cid:{{LogoContentId}}" alt="Allocatr" width="{{LogoWidth}}" height="{{LogoHeight}}" style="display:block;width:{{LogoWidth}}px;height:{{LogoHeight}}px;max-width:100%;border:0;outline:none;text-decoration:none;color:#ffffff;">
                            </td></tr>
                            <tr><td bgcolor="#FBFCF9" class="allocatr-email-content" style="padding:32px 36px;background-color:#FBFCF9;font-family:{{FontFamily}};font-size:14px;font-weight:400;line-height:25px;color:{{Charcoal}};word-wrap:break-word;overflow-wrap:anywhere;">
                                {{content}}
                            </td></tr>
                        </table>
                        <!--[if mso]></td></tr></table><![endif]-->
                        <p class="allocatr-email-muted" style="margin:22px 0 0;font-family:{{FontFamily}};font-size:11px;line-height:20px;color:#667477;">Work, better allocated.</p>
                        <p class="allocatr-email-quiet" style="margin:4px 0 0;font-family:{{FontFamily}};font-size:10px;line-height:18px;color:#667477;">&copy; {{year}} Allocatr &middot; Automated account update</p>
                    </td></tr>
                </table>
            </body>
            </html>
            """;
    }

    public static string Eyebrow(string label)
    {
        var safeLabel = WebUtility.HtmlEncode(label);
        return $"""
            <p class="allocatr-email-accent" style="margin:0 0 14px;font-family:{FontFamily};font-size:10px;font-weight:600;line-height:18px;letter-spacing:1.4px;text-transform:uppercase;color:{Teal};">{safeLabel}</p>
            """;
    }

    /// <summary>Encodes raw project values; never truncate the saved business reference.</summary>
    public static string ProjectCard(string title, string? projectCode = null)
    {
        var safeTitle = WebUtility.HtmlEncode(title);
        var safeCode = WebUtility.HtmlEncode(projectCode?.Trim());
        var reference = string.IsNullOrWhiteSpace(projectCode)
            ? ""
            : $"""
                <tr><td class="allocatr-email-rule" style="padding-top:14px;border-top:1px solid #D9E1DD;">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="width:100%;table-layout:fixed;">
                        <tr>
                            <td class="allocatr-project-reference-label allocatr-email-muted" width="48%" valign="middle" style="width:48%;font-family:{FontFamily};font-size:11px;line-height:20px;color:#667477;">Project reference</td>
                            <td class="allocatr-project-reference-value" align="right" valign="middle" style="text-align:right;">
                                <span class="allocatr-project-code" style="display:inline-block;max-width:100%;padding:3px 8px;border:1px solid #D9E1DD;border-radius:5px;background-color:#F3F5F2;font-family:Consolas,'Courier New',monospace;font-size:11px;line-height:18px;color:#303638;word-break:break-all;">{safeCode}</span>
                            </td>
                        </tr>
                    </table>
                </td></tr>
                """;
        return $"""
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="width:100%;margin-top:24px;table-layout:fixed;">
                <tr><td class="allocatr-project-backing" style="padding:0 0 8px;background-color:#EDF3F3;border-radius:8px;">
                    <table role="presentation" class="allocatr-email-panel" width="100%" cellspacing="0" cellpadding="0" border="0" bgcolor="#FBFCF9" style="width:100%;table-layout:fixed;background-color:#FBFCF9;border:1px solid #AFC1C7;border-left:5px solid {Teal};border-radius:8px;box-shadow:0 5px 12px rgba(3,61,79,0.10);">
                        <tr><td class="allocatr-project-inner" style="padding:20px;">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="width:100%;table-layout:fixed;">
                                <tr><td style="padding-bottom:16px;">
                                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="width:100%;table-layout:fixed;">
                                        <tr>
                                            <td class="allocatr-project-icon-cell" width="56" valign="middle" style="width:56px;padding-right:16px;">
                                                <img class="allocatr-project-icon-light" src="cid:{ProjectIconContentId}" alt="" width="40" height="40" style="display:block;width:40px;height:40px;border:0;border-radius:7px;">
                                                <img class="allocatr-project-icon-dark" src="cid:{DarkProjectIconContentId}" alt="" width="40" height="40" style="display:none;width:40px;height:40px;max-height:0;overflow:hidden;mso-hide:all;border:0;border-radius:7px;">
                                            </td>
                                            <td valign="middle">
                                                <p class="allocatr-email-accent" style="margin:0 0 3px;font-family:{FontFamily};font-size:10px;font-weight:600;line-height:18px;letter-spacing:1.1px;color:{Teal};">PROJECT</p>
                                                <p class="allocatr-project-title allocatr-email-heading" style="margin:0;font-family:{FontFamily};font-size:19px;font-weight:600;line-height:28px;color:#303638;word-break:break-word;overflow-wrap:anywhere;">{safeTitle}</p>
                                            </td>
                                        </tr>
                                    </table>
                                </td></tr>
                                {reference}
                            </table>
                        </td></tr>
                    </table>
                </td></tr>
            </table>
            """;
    }

    /// <summary>Teal action with white text, including a classic Outlook fallback.</summary>
    public static string ActionLink(string label, string url)
    {
        if (
            !Uri.TryCreate(url, UriKind.Absolute, out var target)
            || (target.Scheme != Uri.UriSchemeHttps && target.Scheme != Uri.UriSchemeHttp)
        )
            throw new ArgumentException(
                "Email action links must be absolute HTTP or HTTPS URLs.",
                nameof(url)
            );
        var safeLabel = WebUtility.HtmlEncode(label);
        var safeUrl = WebUtility.HtmlEncode(url);
        // All current labels fit this width. Longer labels remain readable in the HTML fallback.
        var buttonWidth = Math.Max(164, label.Length * 8 + 40);
        return $"""
            <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin:24px 0;">
                <tr><td class="allocatr-action-cell" bgcolor="{Teal}" style="border-radius:6px;background-color:{Teal};">
                    <!--[if mso]><v:roundrect xmlns:v="urn:schemas-microsoft-com:vml" xmlns:w="urn:schemas-microsoft-com:office:word" href="{safeUrl}" style="height:44px;v-text-anchor:middle;width:{buttonWidth}px;" arcsize="12%" stroke="f" fillcolor="{Teal}"><w:anchorlock/><center style="font-family:Arial,Helvetica,sans-serif;font-size:13px;font-weight:bold;color:#ffffff;">{safeLabel}</center></v:roundrect><![endif]-->
                    <!--[if !mso]><!--><a class="allocatr-email-action" href="{safeUrl}" style="display:inline-block;padding:12px 22px;border:1px solid {Teal};border-radius:6px;background-color:{Teal};background-image:linear-gradient({Teal},{Teal});font-family:{FontFamily};font-size:13px;font-weight:600;line-height:20px;color:#ffffff;-webkit-text-fill-color:#ffffff;text-decoration:none;text-align:center;"><span style="color:#ffffff;-webkit-text-fill-color:#ffffff;">{safeLabel}</span></a><!--<![endif]-->
                </td></tr>
            </table>
            """;
    }

    /// <summary>A discreet secondary link; the plain-text email retains the full address.</summary>
    public static string FallbackLink(string url)
    {
        if (
            !Uri.TryCreate(url, UriKind.Absolute, out var target)
            || (target.Scheme != Uri.UriSchemeHttps && target.Scheme != Uri.UriSchemeHttp)
        )
            throw new ArgumentException(
                "Email fallback links must be absolute HTTP or HTTPS URLs.",
                nameof(url)
            );
        var safeUrl = WebUtility.HtmlEncode(url);
        return $"""
            <div class="allocatr-email-rule" style="margin-top:24px;padding-top:18px;border-top:1px solid #D9E1DD;">
                <p class="allocatr-email-muted" style="margin:0;font-family:{FontFamily};font-size:11px;line-height:20px;color:#667477;">If the button does not work, use the link below.</p>
                <p style="margin:6px 0 0;word-break:break-all;overflow-wrap:anywhere;">
                    <a class="allocatr-email-link" href="{safeUrl}" style="font-family:{FontFamily};font-size:11px;line-height:20px;color:{Teal};text-decoration:underline;word-break:break-all;">Open in your browser</a>
                </p>
            </div>
            """;
    }

    private static string DarkRules() =>
        """
            .allocatr-email-canvas { background-color:#0E1518 !important; }
            .allocatr-email-card, .allocatr-email-content { background-color:#161F23 !important; color:#E8ECEC !important; }
            .allocatr-email-card, .allocatr-email-rule { border-color:#334248 !important; }
            .allocatr-email-header { background-color:#303638 !important; }
            .allocatr-email-heading { color:#E8ECEC !important; }
            .allocatr-email-text { color:#CFD7D8 !important; }
            .allocatr-email-muted, .allocatr-email-quiet { color:#A5B0B2 !important; }
            .allocatr-email-accent, .allocatr-email-link { color:#DEDA00 !important; }
            .allocatr-project-backing { background-color:#20251A !important; }
            .allocatr-email-panel { background-color:#192429 !important; border-color:#4D7C88 !important; border-left-color:#DEDA00 !important; box-shadow:0 5px 12px rgba(0,0,0,0.22) !important; }
            .allocatr-project-icon-light { display:none !important; max-height:0 !important; overflow:hidden !important; }
            .allocatr-project-icon-dark { display:block !important; max-height:40px !important; overflow:visible !important; }
            .allocatr-project-code { background-color:#223137 !important; border-color:#4D7C88 !important; color:#E8ECEC !important; }
            .allocatr-action-cell, .allocatr-email-action { background-color:#DEDA00 !important; background-image:linear-gradient(#DEDA00,#DEDA00) !important; border-color:#DEDA00 !important; }
            .allocatr-email-action, .allocatr-email-action span { color:#303030 !important; -webkit-text-fill-color:#303030 !important; }
            """;
}
