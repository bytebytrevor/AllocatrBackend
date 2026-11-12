using System.Net;
using AllocatrApi.Data;
using AllocatrApi.Enums;
using AllocatrApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public sealed class ProjectInvitationEmailSender(
    AllocatrDbContext db,
    UserManager<AllocatrUser> users,
    IEmailService email,
    IConfiguration configuration)
{
    public async Task SendAsync(Guid projectId, Guid allocatUserId, Guid ownerId)
    {
        // Resolve the saved invitation and account on the server. The caller
        // never supplies a recipient address or an arbitrary email link.
        var invitation = await db.ProjectAllocats.AsNoTracking()
            .Where(item => item.ProjectId == projectId
                && item.AllocatProfileId == allocatUserId
                && item.Status == ProjectAllocatStatus.Invited
                && item.RemovedAt == null
                && item.Project.UserId == ownerId)
            .Select(item => new { Title = item.Project.Title })
            .SingleOrDefaultAsync();
        if (invitation is null) return;

        var recipient = await users.FindByIdAsync(allocatUserId.ToString());
        if (recipient is null || string.IsNullOrWhiteSpace(recipient.Email)) return;
        var client = await users.FindByIdAsync(ownerId.ToString());
        var clientName = string.IsNullOrWhiteSpace(client?.FullName)
            ? "A client" : client!.FullName.Trim();
        var recipientName = string.IsNullOrWhiteSpace(recipient.FullName)
            ? "there" : recipient.FullName.Trim();

        var configured = configuration["ProjectInvitations:FrontendBaseUrl"]
            ?? configuration["EmailVerification:FrontendBaseUrl"]
            ?? configuration["PasswordReset:FrontendBaseUrl"];
        if (!Uri.TryCreate(configured, UriKind.Absolute, out var origin)
            || origin.AbsolutePath != "/" || !string.IsNullOrEmpty(origin.Query)
            || !string.IsNullOrEmpty(origin.Fragment) || !string.IsNullOrEmpty(origin.UserInfo)
            || (origin.Scheme != Uri.UriSchemeHttps
                && !(origin.Scheme == Uri.UriSchemeHttp && origin.IsLoopback)))
            throw new InvalidOperationException(
                "Configure ProjectInvitations:FrontendBaseUrl with the frontend HTTPS origin (localhost HTTP is allowed).");

        // The current frontend exposes invitations through My work. No public
        // project access, acceptance action, or bearer token is added by this link.
        var url = origin.AbsoluteUri.TrimEnd('/') + "/projects";
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeClient = WebUtility.HtmlEncode(clientName);
        var safeTitle = WebUtility.HtmlEncode(invitation.Title);
        var safeUrl = WebUtility.HtmlEncode(url);
        var action = EmailLayout.ActionLink("View invitations", url);
        var html = $"""
            <h1 style="margin:0;font-family:{EmailLayout.FontFamily};font-size:24px;font-weight:600;line-height:32px;letter-spacing:-0.5px;color:#333333;">You’re invited to a project</h1>
            <p style="margin:20px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#555d52;">Hi {safeName},</p>
            <p style="margin:10px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#555d52;">{safeClient} has invited you to join a project on Allocatr.</p>
            <div style="margin-top:20px;padding:16px;border:1px solid #e1e4dc;border-radius:8px;background-color:#f3f4f1;">
                <p style="margin:0;font-family:{EmailLayout.FontFamily};font-size:11px;line-height:20px;color:#697066;">Project</p>
                <p style="margin:4px 0 0;font-family:{EmailLayout.FontFamily};font-size:16px;font-weight:600;line-height:25px;word-break:break-word;color:#333333;">{safeTitle}</p>
            </div>
            {action}
            <p style="margin:0;font-family:{EmailLayout.FontFamily};font-size:12px;line-height:22px;color:#697066;">Sign in with the account receiving this email, then open <strong>My work → Invitations</strong>. Review the project brief and supporting files before accepting or declining.</p>
            <p style="margin:10px 0 0;font-family:{EmailLayout.FontFamily};font-size:12px;line-height:22px;color:#697066;">Opening this link does not accept the invitation.</p>
            <div style="margin-top:24px;padding-top:18px;border-top:1px solid #e6e9e1;">
                <p style="margin:0;font-family:{EmailLayout.FontFamily};font-size:11px;line-height:20px;color:#82877e;">If the button does not work, copy this link into your browser:</p>
                <p style="margin:6px 0 0;font-family:{EmailLayout.FontFamily};font-size:11px;line-height:20px;word-break:break-all;color:#697066;">{safeUrl}</p>
            </div>
            """;
        var text = $"Hi {recipientName},\n\n{clientName} has invited you to join this project on Allocatr:\n{invitation.Title}\n\n{url}\n\nSign in with the account receiving this email, then open My work > Invitations. Review the project brief and supporting files before accepting or declining. Opening this link does not accept the invitation.\n\nAllocatr";
        // Keep the subject static so project/user input is confined to encoded
        // HTML and plain-text bodies, not email headers.
        await email.SendAsync(recipient.Email, "You have a project invitation on Allocatr", html, text);
    }
}
