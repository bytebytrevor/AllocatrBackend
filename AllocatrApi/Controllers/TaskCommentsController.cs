using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks/{taskId:guid}/comments")]
public class TaskCommentsController : ControllerBase
{
    private readonly TaskCommentService _comments;
    private readonly UserManager<AllocatrUser> _users;
    public TaskCommentsController(TaskCommentService commentService, UserManager<AllocatrUser> userManager)
    { _comments = commentService; _users = userManager; }

    [HttpGet]
    public Task<ActionResult<List<TaskCommentDto>>> GetComments(Guid taskId) =>
        RunAsync<List<TaskCommentDto>>(async user =>
        {
            var items = await _comments.GetTaskCommentsAsync(taskId, user.Id, user.IsAllocat);
            return items == null ? NotFound(new { message = "Task not found." }) : Ok(items);
        });

    [HttpGet("context")]
    public Task<ActionResult<TaskDiscussionContextDto>> GetContext(Guid taskId) =>
        RunAsync<TaskDiscussionContextDto>(async user =>
        {
            var context = await _comments.GetContextAsync(taskId, user.Id, user.IsAllocat);
            return context == null ? NotFound(new { message = "Task not found." }) : Ok(context);
        });

    [HttpPost]
    public Task<ActionResult<TaskCommentDto>> CreateComment(Guid taskId, [FromBody] CreateTaskCommentDto request) =>
        RunAsync<TaskCommentDto>(async user =>
        {
            var comment = await _comments.CreateCommentAsync(taskId, user.Id, user.IsAllocat, request);
            return comment == null ? NotFound(new { message = "Task not found." })
                : CreatedAtAction(nameof(GetComments), new { taskId }, comment);
        });

    [HttpPatch("{commentId:guid}")]
    public Task<ActionResult<TaskCommentDto>> UpdateComment(Guid taskId, Guid commentId,
        [FromBody] UpdateTaskCommentDto request) => RunAsync<TaskCommentDto>(async user =>
        {
            var comment = await _comments.UpdateCommentAsync(taskId, commentId, user.Id, user.IsAllocat, request.Comment);
            return comment == null ? NotFound(new { message = "Comment not found." }) : Ok(comment);
        });

    [HttpDelete("{commentId:guid}")]
    public Task<ActionResult<object>> DeleteComment(Guid taskId, Guid commentId) =>
        RunAsync<object>(async user => await _comments.DeleteCommentAsync(taskId, commentId, user.Id, user.IsAllocat)
            ? NoContent() : NotFound(new { message = "Comment not found." }));

    private async Task<ActionResult<T>> RunAsync<T>(Func<AllocatrUser, Task<ActionResult>> action)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();
        try { return await action(user); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
        catch (ProjectFileException exception)
        { return Problem(statusCode: exception.StatusCode, title: "Comment request failed", detail: exception.Message); }
    }
}
