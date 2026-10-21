using System.Net.Http;
using Microsoft.Extensions.Configuration;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/conversations")]
public class ProjectConversationsController : ControllerBase
{
    private readonly ProjectMessagingService _messaging;
    private readonly UserManager<AllocatrUser> _users;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClients;
    private bool CanModerate => !string.IsNullOrWhiteSpace(_configuration["Messaging:ModeratorRole"])
        && User.IsInRole(_configuration["Messaging:ModeratorRole"]!);
    public ProjectConversationsController(ProjectMessagingService messaging, UserManager<AllocatrUser> users, IConfiguration configuration, IHttpClientFactory httpClients)
    { _messaging = messaging; _users = users; _configuration = configuration; _httpClients = httpClients; }

    // Expected errors are handled locally; internal exception details never enter this contract.
    private async Task<IActionResult> Run(Func<AllocatrUser, Task<IActionResult>> action)
    {
        Response.Headers["Cache-Control"] = "no-store";
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();
        try { return await action(user); }
        catch (MessagingException e) { return StatusCode(e.StatusCode, new { message = e.Message }); }
        catch (ProjectFileException e) { return StatusCode(e.StatusCode, new { message = e.Message }); }
    }

    [HttpGet("members")]
    public Task<IActionResult> Members(Guid projectId, CancellationToken ct) => Run(async user =>
        Ok(await _messaging.MembersAsync(projectId, user.Id, user.IsAllocat, ct)));

    [HttpGet]
    public Task<IActionResult> List(Guid projectId, [FromQuery] string? search, [FromQuery] string? filter,
        [FromQuery] int page = 1, CancellationToken ct = default) => Run(async user =>
        Ok(await _messaging.ListAsync(projectId, user.Id, user.IsAllocat, search, filter, page, ct)));

    [HttpGet("{conversationId:guid}")]
    public Task<IActionResult> Detail(Guid projectId, Guid conversationId, [FromQuery] long? before, CancellationToken ct) => Run(async user =>
        Ok(await _messaging.DetailAsync(projectId, conversationId, user.Id, user.IsAllocat, before, ct, CanModerate)));

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ProjectMessagingService.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ProjectMessagingService.MaxRequestBytes)]
    public Task<IActionResult> Create(Guid projectId, [FromForm] CreateConversationForm form, CancellationToken ct) => Run(async user =>
        Ok(await _messaging.CreateAsync(projectId, user.Id, user.IsAllocat, form, ct)));

    [HttpPost("{conversationId:guid}/messages")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ProjectMessagingService.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ProjectMessagingService.MaxRequestBytes)]
    public Task<IActionResult> Reply(Guid projectId, Guid conversationId, [FromForm] ReplyConversationForm form, CancellationToken ct) => Run(async user =>
        Ok(await _messaging.ReplyAsync(projectId, conversationId, user.Id, user.IsAllocat, form, ct)));

    [HttpPost("{conversationId:guid}/read")]
    public Task<IActionResult> Read(Guid projectId, Guid conversationId, [FromBody] ConversationReadDto form, CancellationToken ct) => Run(async user =>
    {
        await _messaging.MarkReadAsync(projectId, conversationId, user.Id, user.IsAllocat, form.ThroughSequence, ct);
        return NoContent();
    });

    [HttpPatch("{conversationId:guid}/archive")]
    public Task<IActionResult> Archive(Guid projectId, Guid conversationId, [FromBody] ConversationArchiveDto form, CancellationToken ct) => Run(async user =>
    {
        await _messaging.ArchiveAsync(projectId, conversationId, user.Id, user.IsAllocat, form.Archived, ct);
        return NoContent();
    });

    [HttpPatch("{conversationId:guid}/pin")]
    public Task<IActionResult> Pin(Guid projectId, Guid conversationId,
        [FromBody] ConversationPinDto form, CancellationToken ct) => Run(async user =>
        {
            await _messaging.PinAsync(projectId, conversationId, user.Id, user.IsAllocat, form.Pinned, ct);
            return NoContent();
        });

    [HttpPatch("{conversationId:guid}/messages/{messageId:guid}")]
    public Task<IActionResult> Edit(Guid projectId, Guid conversationId, Guid messageId,
        [FromBody] ConversationEditDto form, CancellationToken ct) => Run(async user =>
        {
            await _messaging.EditAsync(projectId, conversationId, messageId, user.Id, user.IsAllocat, form, ct);
            return NoContent();
        });

    [HttpPost("{conversationId:guid}/messages/{messageId:guid}/delete")]
    public Task<IActionResult> Delete(Guid projectId, Guid conversationId, Guid messageId,
        [FromBody] ConversationDeleteDto form, CancellationToken ct) => Run(async user =>
        {
            await _messaging.DeleteAsync(projectId, conversationId, messageId, user.Id, user.IsAllocat,
                CanModerate, form, ct);
            return NoContent();
        });

    [HttpPost("{conversationId:guid}/messages/{messageId:guid}/forward")]
    public Task<IActionResult> Forward(Guid projectId, Guid conversationId, Guid messageId,
        [FromBody] ConversationForwardDto form, CancellationToken ct) => Run(async user =>
        Ok(await _messaging.ForwardAsync(projectId, conversationId, messageId, user.Id, user.IsAllocat, form, ct)));

    [HttpGet("{conversationId:guid}/files/{fileId:guid}/download")]
    public Task<IActionResult> Download(Guid projectId, Guid conversationId, Guid fileId, CancellationToken ct) => Run(async user =>
    {
        var file = await _messaging.DownloadAsync(projectId, conversationId, fileId, user.Id, user.IsAllocat, _httpClients, ct);
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(file.Bytes, file.ContentType, file.Name);
    });
}
