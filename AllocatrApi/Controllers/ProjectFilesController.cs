using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AllocatrApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/files")]
public class ProjectFilesController : ControllerBase
{
    private readonly UserManager<AllocatrUser> _users;
    private readonly ProjectFileService _files;
    private readonly ILogger<ProjectFilesController> _logger;

    public ProjectFilesController(UserManager<AllocatrUser> users,
        ProjectFileService files, ILogger<ProjectFilesController> logger)
    {
        _users = users;
        _files = files;
        _logger = logger;
    }

    [HttpGet]
    public Task<ActionResult<ProjectFilePageDto>> List(Guid projectId,
        [FromQuery] Guid? taskId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50, [FromQuery] string? search = null,
        [FromQuery] string? source = null, CancellationToken cancellationToken = default,
        [FromQuery] string? sort = null, [FromQuery] string? kind = null) =>
        RunAsync<ProjectFilePageDto>(async user => Ok(await _files.ListAsync(
            projectId, user.Id, user.IsAllocat, taskId, page, pageSize, cancellationToken, search, source, sort, kind)));

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ProjectFileValidationService.MaxRequestSizeBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ProjectFileValidationService.MaxRequestSizeBytes)]
    public Task<ActionResult<ProjectFileDto>> Upload(Guid projectId,
        [FromForm] IFormFile file, [FromForm] Guid? taskId,
        CancellationToken cancellationToken = default) =>
        RunAsync<ProjectFileDto>(async user =>
        {
            var uploaded = await _files.UploadAsync(projectId, user.Id,
                user.IsAllocat, taskId, file, cancellationToken);
            return CreatedAtAction(nameof(Download),
                new { projectId, fileId = uploaded.Id }, uploaded);
        });

    [HttpGet("{fileId:guid}/download")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public Task<ActionResult<ProjectFileDownloadDto>> Download(Guid projectId,
        Guid fileId, CancellationToken cancellationToken = default) =>
        RunAsync<ProjectFileDownloadDto>(async user => Ok(await _files.GetDownloadAsync(
            projectId, fileId, user.Id, user.IsAllocat, cancellationToken)));

    [HttpDelete("{fileId:guid}")]
    public Task<ActionResult<object>> Delete(Guid projectId, Guid fileId,
        CancellationToken cancellationToken = default) =>
        RunAsync<object>(async user =>
        {
            await _files.DeleteAsync(projectId, fileId, user.Id,
                user.IsAllocat, cancellationToken);
            return NoContent();
        });

    private async Task<ActionResult<T>> RunAsync<T>(Func<AllocatrUser, Task<ActionResult>> action)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();
        try { return await action(user); }
        catch (ProjectFileException exception)
        {
            return Problem(statusCode: exception.StatusCode,
                title: "Project file request failed", detail: exception.Message);
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Project file request failed for user {UserId}", user.Id);
            return Problem(statusCode: 503, title: "File service temporarily unavailable",
                detail: "The file request could not be completed. Please try again.");
        }
    }
}
