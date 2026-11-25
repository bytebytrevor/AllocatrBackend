using System.Globalization;
using System.Security.Claims;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class NotificationsController(NotificationService notifications) : ControllerBase
{
    private bool TryUser(out Guid id) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out id);

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? before,
        [FromQuery] int take = 30,
        CancellationToken ct = default
    )
    {
        if (!TryUser(out var userId))
            return Unauthorized();
        long? cursor = null;
        if (before is not null)
        {
            if (!PositiveId(before, out var value))
                return BadRequest(new { message = "Invalid notification cursor." });
            cursor = value;
        }
        return Ok(await notifications.ListAsync(userId, cursor, take, ct));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        if (!TryUser(out var userId))
            return Unauthorized();
        return Ok(new { unreadCount = await notifications.UnreadCountAsync(userId, ct) });
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> Read(string id, CancellationToken ct)
    {
        if (!TryUser(out var userId))
            return Unauthorized();
        if (!PositiveId(id, out var value))
            return BadRequest();
        await notifications.MarkReadAsync(userId, value, ct);
        // No disclosure of whether an ID belongs to someone else.
        return NoContent();
    }

    public sealed record ReadAllRequest(string ThroughId);

    [HttpPatch("read-all")]
    public async Task<IActionResult> ReadAll(ReadAllRequest request, CancellationToken ct)
    {
        if (!TryUser(out var userId))
            return Unauthorized();
        if (!PositiveId(request.ThroughId, out var value))
            return BadRequest();
        await notifications.MarkAllReadAsync(userId, value, ct);
        return NoContent();
    }

    private static bool PositiveId(string? input, out long value) =>
        long.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out value)
        && value > 0;
}
