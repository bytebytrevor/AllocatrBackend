using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/files/{fileId:guid}/thumbnail")]
public class ProjectFileThumbnailsController : ControllerBase
{
    private readonly UserManager<AllocatrUser> _users;
    private readonly ProjectThumbnailService _thumbnails;
    public ProjectFileThumbnailsController(UserManager<AllocatrUser> users, ProjectThumbnailService thumbnails)
    { _users = users; _thumbnails = thumbnails; }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Get(Guid projectId, Guid fileId, CancellationToken cancellationToken)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();
        try
        {
            var bytes = await _thumbnails.GetAsync(projectId, fileId, user.Id, user.IsAllocat, cancellationToken);
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(bytes, "image/jpeg");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return new EmptyResult(); }
        catch (ProjectFileException exception) { return Problem(statusCode: exception.StatusCode, detail: exception.Message); }
    }
}
