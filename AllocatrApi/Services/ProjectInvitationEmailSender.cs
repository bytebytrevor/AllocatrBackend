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
    IConfiguration configuration
)
{
    public async Task SendAsync(Guid projectId, Guid allocatUserId, Guid ownerId)
    {
        // Resolve the saved invitation and account on the server. The caller
        // never supplies a recipient address or an arbitrary email link.
        var invitation = await db
            .ProjectAllocats.AsNoTracking()
            .Where(item =>
                item.ProjectId == projectId
                && item.AllocatProfileId == allocatUserId
                && item.Status == ProjectAllocatStatus.Invited
                && item.RemovedAt == null
                && item.Project.UserId == ownerId
            )
            .Select(item => new { Title = item.Project.Title, item.Project.ProjectCode })
            .SingleOrDefaultAsync();
        if (invitation is null)
            return;

        var recipient = await users.FindByIdAsync(allocatUserId.ToString());
        if (recipient is null || string.IsNullOrWhiteSpace(recipient.Email))
            return;
        var client = await users.FindByIdAsync(ownerId.ToString());
        var clientName = string.IsNullOrWhiteSpace(client?.FullName)
            ? "A client"
            : client!.FullName.Trim();
        var recipientName = string.IsNullOrWhiteSpace(recipient.FullName)
            ? "there"
            : recipient.FullName.Trim();

        var origin = FrontendOrigin();

        // The current frontend exposes invitations through My work. No public
        // project access, acceptance action, or bearer token is added by this link.
        var url = origin.AbsoluteUri.TrimEnd('/') + "/projects";
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeClient = WebUtility.HtmlEncode(clientName);
        var projectCard = EmailLayout.ProjectCard(invitation.Title, invitation.ProjectCode);
        var eyebrow = EmailLayout.Eyebrow("Project invitation");
        var fallback = EmailLayout.FallbackLink(url);
        var action = EmailLayout.ActionLink("View invitations", url);
        var html = $"""
            {eyebrow}
            <h1 class="allocatr-email-heading" style="margin:0;font-family:{EmailLayout.FontFamily};font-size:26px;font-weight:600;line-height:35px;letter-spacing:-0.5px;color:#303638;">You’re invited to a project</h1>
            <p class="allocatr-email-text" style="margin:20px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#47575C;">Hi {safeName},</p>
            <p class="allocatr-email-text" style="margin:10px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#47575C;">{safeClient} has invited you to join a project on Allocatr.</p>
            {projectCard}
            {action}
            <p class="allocatr-email-muted" style="margin:0;font-family:{EmailLayout.FontFamily};font-size:12px;line-height:22px;color:#667477;">Sign in with the account receiving this email, then open <strong>My work → Invitations</strong>. Review the project brief and supporting files before accepting or declining.</p>
            <p class="allocatr-email-muted" style="margin:10px 0 0;font-family:{EmailLayout.FontFamily};font-size:12px;line-height:22px;color:#667477;">Opening this link does not accept the invitation.</p>
            {fallback}
            """;
        var text =
            $"Hi {recipientName},\n\n{clientName} has invited you to join this project on Allocatr:\n{invitation.Title}\n\n{url}\n\nSign in with the account receiving this email, then open My work > Invitations. Review the project brief and supporting files before accepting or declining. Opening this link does not accept the invitation.\n\nAllocatr";
        // Keep the subject static so project/user input is confined to encoded
        // HTML and plain-text bodies, not email headers.
        await email.SendAsync(
            recipient.Email,
            "You have a project invitation on Allocatr",
            html,
            text
        );
    }

    public async Task SendResponseAsync(Guid projectId, Guid allocatUserId, bool accepted)
    {
        var expectedStatus = accepted
            ? ProjectAllocatStatus.Accepted
            : ProjectAllocatStatus.Declined;
        // Derive the client recipient from the saved project relationship.
        // No request-supplied address, owner ID, name or redirect is trusted.
        var response = await db
            .ProjectAllocats.AsNoTracking()
            .Where(item =>
                item.ProjectId == projectId
                && item.AllocatProfile.AllocatrUserId == allocatUserId
                && item.Status == expectedStatus
                && item.RespondedAt != null
                && item.RemovedAt == null
                && item.Project.UserId != allocatUserId
            )
            .Select(item => new
            {
                OwnerId = item.Project.UserId,
                item.Project.Title,
                item.Project.ProjectCode,
                AllocatName = item.AllocatProfile.AllocatrUser.FullName,
            })
            .SingleOrDefaultAsync();
        if (response is null)
            return;

        var client = await users.FindByIdAsync(response.OwnerId.ToString());
        if (client is null || string.IsNullOrWhiteSpace(client.Email))
            return;

        var clientName = string.IsNullOrWhiteSpace(client.FullName)
            ? "there"
            : client.FullName.Trim();
        var allocatName = string.IsNullOrWhiteSpace(response.AllocatName)
            ? "An Allocat"
            : response.AllocatName.Trim();
        var decision = accepted ? "accepted" : "declined";
        var title = accepted ? "Invitation accepted" : "Invitation declined";
        var subject = accepted
            ? "Your project invitation was accepted"
            : "Your project invitation was declined";
        var url =
            FrontendOrigin(forResponse: true).AbsoluteUri.TrimEnd('/') + $"/projects/{projectId}";
        var safeClient = WebUtility.HtmlEncode(clientName);
        var safeAllocat = WebUtility.HtmlEncode(allocatName);
        var projectCard = EmailLayout.ProjectCard(response.Title, response.ProjectCode);
        var eyebrow = EmailLayout.Eyebrow("Project invitation");
        var fallback = EmailLayout.FallbackLink(url);
        var action = EmailLayout.ActionLink("Open project", url);
        var html = $"""
            {eyebrow}
            <h1 class="allocatr-email-heading" style="margin:0;font-family:{EmailLayout.FontFamily};font-size:26px;font-weight:600;line-height:35px;letter-spacing:-0.5px;color:#303638;">{title}</h1>
            <p class="allocatr-email-text" style="margin:20px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#47575C;">Hi {safeClient},</p>
            <p class="allocatr-email-text" style="margin:10px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#47575C;">{safeAllocat} has {decision} your invitation to join this project.</p>
            {projectCard}
            {action}
            <p class="allocatr-email-muted" style="margin:0;font-family:{EmailLayout.FontFamily};font-size:12px;line-height:22px;color:#667477;">Sign in with your client account to view the project.</p>
            {fallback}
            """;
        var code = string.IsNullOrWhiteSpace(response.ProjectCode)
            ? ""
            : $"\n{response.ProjectCode.Trim()}";
        var text =
            $"Hi {clientName},\n\n{allocatName} has {decision} your invitation to join this project:\n{response.Title}{code}\n\nOpen project: {url}\n\nSign in with your client account to view the project.\n\nAllocatr";

        // Subject and decision wording are fixed strings; user input is encoded
        // in the HTML body. The existing EmailService adds the shared layout.
        await email.SendAsync(client.Email, subject, html, text);
    }

    public async Task SendLifecycleAsync(Guid eventId, Guid recipientId)
    {
        // Recipient membership at the event is saved server-side; no supplied address is trusted.
        var item = await db.Set<ProjectLifecycleEvent>()
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == eventId && e.RecipientIds.Contains(recipientId));
        if (item == null)
            return;
        var recipient = await users.FindByIdAsync(recipientId.ToString());
        if (recipient == null || string.IsNullOrWhiteSpace(recipient.Email))
            return;
        var cancelled = item.Kind == "project-cancelled";
        if (!cancelled && item.Kind != "allocat-left")
            return;
        var title = cancelled ? "Project cancelled" : "An Allocat left your project";
        var subject = cancelled
            ? "A project you joined was cancelled"
            : "An Allocat left your project";
        var name = string.IsNullOrWhiteSpace(recipient.FullName)
            ? "there"
            : recipient.FullName.Trim();
        var safeName = WebUtility.HtmlEncode(name);
        var safeActor = WebUtility.HtmlEncode(item.ActorName);
        var safeReason = WebUtility.HtmlEncode(item.Reason);
        var message = cancelled
            ? $"{safeActor} cancelled this project. Work has stopped."
            : $"{safeActor} left this project. Their unfinished tasks are now unassigned.";
        var guidance = cancelled
            ? "You can still view the project history and discuss outstanding questions with the client."
            : "The project remains open. You can invite another Allocat to continue the work. Unfinished tasks are now unassigned.";
        var url =
            FrontendOrigin(forResponse: true).AbsoluteUri.TrimEnd('/')
            + $"/projects/{item.ProjectId}";
        var eyebrow = EmailLayout.Eyebrow("Project update");
        var projectCard = EmailLayout.ProjectCard(item.ProjectTitle, item.ProjectCode);
        var action = EmailLayout.ActionLink("View project", url);
        var fallback = EmailLayout.FallbackLink(url);
        var html = $"""
            {eyebrow}
            <h1 class="allocatr-email-heading" style="margin:0;font-family:{EmailLayout.FontFamily};font-size:26px;font-weight:600;line-height:35px;color:#303638;">{title}</h1>
            <p class="allocatr-email-text" style="margin:20px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#47575C;">Hi {safeName},</p>
            <p class="allocatr-email-text" style="margin:10px 0 0;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#47575C;">{message}</p>
            {projectCard}
            <p class="allocatr-email-muted" style="margin:20px 0 4px;font-family:{EmailLayout.FontFamily};font-size:11px;line-height:20px;color:#667477;">Reason provided</p>
            <p class="allocatr-email-text" style="margin:0;white-space:pre-line;font-family:{EmailLayout.FontFamily};font-size:14px;line-height:25px;color:#47575C;">{safeReason}</p>
            {action}
            <p class="allocatr-email-muted" style="margin:0;font-family:{EmailLayout.FontFamily};font-size:12px;line-height:22px;color:#667477;">{guidance}</p>
            {fallback}
            """;
        var text =
            $"Hi {name},\n\n{item.ActorName} {(cancelled ? "cancelled" : "left")} this project:\n{item.ProjectTitle}\n{item.ProjectCode}\n\nReason: {item.Reason}\n\n{guidance}\n\nView project: {url}\n\nAllocatr";
        await email.SendAsync(recipient.Email, subject, html, text);
    }

    private Uri FrontendOrigin(bool forResponse = false)
    {
        var configured =
            (forResponse ? configuration["ProjectInvitationResponses:FrontendBaseUrl"] : null)
            ?? configuration["ProjectInvitations:FrontendBaseUrl"]
            ?? configuration["EmailVerification:FrontendBaseUrl"]
            ?? configuration["PasswordReset:FrontendBaseUrl"];
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
                forResponse
                    ? "Configure ProjectInvitationResponses:FrontendBaseUrl or ProjectInvitations:FrontendBaseUrl with the frontend HTTPS origin (localhost HTTP is allowed)."
                    : "Configure ProjectInvitations:FrontendBaseUrl with the frontend HTTPS origin (localhost HTTP is allowed)."
            );
        return origin;
    }
}
