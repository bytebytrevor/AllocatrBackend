using System.Net;
using System.Text;
using AllocatrApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace AllocatrApi.Services;

public sealed class PasswordResetSender(UserManager<AllocatrUser> users,
    IEmailService email, IConfiguration configuration)
{
    public async Task SendAsync(AllocatrUser user)
    {
        if (string.IsNullOrWhiteSpace(user.Email)) return;
        var configured = configuration["PasswordReset:FrontendBaseUrl"]
            ?? configuration["EmailVerification:FrontendBaseUrl"];
        if (!Uri.TryCreate(configured, UriKind.Absolute, out var origin)
            || origin.AbsolutePath != "/" || !string.IsNullOrEmpty(origin.Query)
            || !string.IsNullOrEmpty(origin.Fragment) || !string.IsNullOrEmpty(origin.UserInfo)
            || (origin.Scheme != Uri.UriSchemeHttps && !(origin.Scheme == Uri.UriSchemeHttp && origin.IsLoopback)))
            throw new InvalidOperationException("Configure PasswordReset:FrontendBaseUrl or EmailVerification:FrontendBaseUrl with the frontend HTTPS origin (localhost HTTP is allowed).");

        var token = await users.GeneratePasswordResetTokenAsync(user);
        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        // Fragment values stay out of ordinary frontend-server request logs.
        var url = origin.AbsoluteUri.TrimEnd('/') + "/reset-password#userId="
            + Uri.EscapeDataString(user.Id.ToString()) + "&token=" + Uri.EscapeDataString(encoded);
        var safeName = WebUtility.HtmlEncode(user.FullName);
        var safeUrl = WebUtility.HtmlEncode(url);
        var action = EmailLayout.ActionLink("Reset password", url);
        var html = $"""
            <h1 style="margin:0;font-family:{EmailLayout.FontFamily};font-size:24px;font-weight:600;line-height:32px;letter-spacing:-0.5px;color:#333333;">Reset your password</h1>
            <p style="margin:20px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#555d52;">Hi {safeName},</p>
            <p style="margin:10px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#555d52;">Choose a new password to get back to your Allocatr workspace.</p>
            {action}
            <p style="margin:0;font-family:{EmailLayout.FontFamily};font-size:12px;line-height:22px;color:#697066;">This reset link expires in one hour.</p>
            <p style="margin:10px 0 0;font-family:{EmailLayout.FontFamily};font-size:12px;line-height:22px;color:#697066;">If you didn’t request this, you can ignore this email. Your password stays unchanged.</p>
            <div style="margin-top:24px;padding-top:18px;border-top:1px solid #e6e9e1;">
                <p style="margin:0;font-family:{EmailLayout.FontFamily};font-size:11px;line-height:20px;color:#82877e;">If the button does not work, copy this link into your browser:</p>
                <p style="margin:6px 0 0;font-family:{EmailLayout.FontFamily};font-size:11px;line-height:20px;word-break:break-all;color:#697066;">{safeUrl}</p>
            </div>
            """;
        var text = $"Hi {user.FullName},\n\nReset your Allocatr password using this link:\n{url}\n\nThis link expires in one hour. If you did not request this, ignore this email. Your password stays unchanged.\n\nAllocatr";
        await email.SendAsync(user.Email, "Reset your Allocatr password", html, text);
    }
}
