using System.Net;
using System.Text;
using AllocatrApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace AllocatrApi.Services;

public sealed class PasswordResetSender(
    UserManager<AllocatrUser> users,
    IEmailService email,
    IConfiguration configuration
)
{
    public async Task SendAsync(AllocatrUser user)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
            return;
        var configured =
            configuration["PasswordReset:FrontendBaseUrl"]
            ?? configuration["EmailVerification:FrontendBaseUrl"];
        if (
            !Uri.TryCreate(configured, UriKind.Absolute, out var origin)
            || origin.AbsolutePath != "/"
            || !string.IsNullOrEmpty(origin.Query)
            || !string.IsNullOrEmpty(origin.Fragment)
            || !string.IsNullOrEmpty(origin.UserInfo)
            || (
                origin.Scheme != Uri.UriSchemeHttps
                && !(origin.Scheme == Uri.UriSchemeHttp && origin.IsLoopback)
            )
        )
            throw new InvalidOperationException(
                "Configure PasswordReset:FrontendBaseUrl or EmailVerification:FrontendBaseUrl with the frontend HTTPS origin (localhost HTTP is allowed)."
            );

        var token = await users.GeneratePasswordResetTokenAsync(user);
        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        // Fragment values stay out of ordinary frontend-server request logs.
        var url =
            origin.AbsoluteUri.TrimEnd('/')
            + "/reset-password#userId="
            + Uri.EscapeDataString(user.Id.ToString())
            + "&token="
            + Uri.EscapeDataString(encoded);
        var safeName = WebUtility.HtmlEncode(user.FullName);
        var eyebrow = EmailLayout.Eyebrow("Account security");
        var fallback = EmailLayout.FallbackLink(url);
        var action = EmailLayout.ActionLink("Reset password", url);
        var html = $"""
            {eyebrow}
            <h1 class="allocatr-email-heading" style="margin:0;font-family:{EmailLayout.FontFamily};font-size:26px;font-weight:600;line-height:35px;letter-spacing:-0.5px;color:#303638;">Reset your password</h1>
            <p class="allocatr-email-text" style="margin:20px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#47575C;">Hi {safeName},</p>
            <p class="allocatr-email-text" style="margin:10px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#47575C;">Choose a new password to get back to your Allocatr workspace.</p>
            {action}
            <p class="allocatr-email-muted" style="margin:0;font-family:{EmailLayout.FontFamily};font-size:12px;line-height:22px;color:#667477;">This reset link expires in one hour.</p>
            <p class="allocatr-email-muted" style="margin:10px 0 0;font-family:{EmailLayout.FontFamily};font-size:12px;line-height:22px;color:#667477;">If you didn’t request this, you can ignore this email. Your password stays unchanged.</p>
            {fallback}
            """;
        var text =
            $"Hi {user.FullName},\n\nReset your Allocatr password using this link:\n{url}\n\nThis link expires in one hour. If you did not request this, ignore this email. Your password stays unchanged.\n\nAllocatr";
        await email.SendAsync(user.Email, "Reset your Allocatr password", html, text);
    }
}
