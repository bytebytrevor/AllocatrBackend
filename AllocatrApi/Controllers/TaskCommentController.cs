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
    private readonly TaskCommentService _commentService;
    private readonly UserManager<AllocatrUser> _userManager;

    public TaskCommentsController(
        TaskCommentService commentService,
        UserManager<AllocatrUser> userManager
    )
    {
        _commentService = commentService;
        _userManager = userManager;
    }

    /* --------------------------------------------------------
     * READ
     * -------------------------------------------------------- */

    [HttpGet]
    public async Task<ActionResult<List<TaskCommentDto>>> GetComments(
        Guid taskId
    )
    {
        var comments =
            await _commentService.GetTaskCommentsAsync(taskId);

        return Ok(comments);
    }

    /* --------------------------------------------------------
     * CREATE
     * -------------------------------------------------------- */

    [HttpPost]
    public async Task<ActionResult<TaskCommentDto>> CreateComment(
        Guid taskId,
        [FromBody] CreateTaskCommentDto request
    )
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        try
        {
            var comment =
                await _commentService.CreateCommentAsync(
                    taskId,
                    user.Id,
                    request
                );

            if (comment == null)
            {
                return NotFound(
                    new
                    {
                        message = "Task not found."
                    }
                );
            }

            return CreatedAtAction(
                nameof(GetComments),
                new
                {
                    taskId
                },
                comment
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                }
            );
        }
    }

    /* --------------------------------------------------------
     * UPDATE
     * -------------------------------------------------------- */

    [HttpPatch("{commentId:guid}")]
    public async Task<ActionResult<TaskCommentDto>> UpdateComment(
        Guid taskId,
        Guid commentId,
        [FromBody] UpdateTaskCommentDto request
    )
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        try
        {
            var updatedComment =
                await _commentService.UpdateCommentAsync(
                    taskId,
                    commentId,
                    user.Id,
                    request.Comment
                );

            if (updatedComment == null)
            {
                return NotFound(
                    new
                    {
                        message =
                            "Comment not found or you do not have permission to edit it."
                    }
                );
            }

            /*
             * Defensive check because taskId exists in the route.
             * This prevents accidentally returning a comment belonging
             * to a different task if a mismatched URL is supplied.
             */
            if (updatedComment.TaskItemId != taskId)
            {
                return NotFound(
                    new
                    {
                        message =
                            "Comment was not found for this task."
                    }
                );
            }

            return Ok(updatedComment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                }
            );
        }
    }

    /* --------------------------------------------------------
     * DELETE
     * -------------------------------------------------------- */

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(
        Guid taskId,
        Guid commentId
    )
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        /*
         * Because taskId is also part of the route, the
         * service should additionally verify TaskItemId == taskId.
         */

        var deleted =
            await _commentService.DeleteCommentAsync(
                taskId,
                commentId,
                user.Id
            );

        if (!deleted)
        {
            return NotFound(
                new
                {
                    message =
                        "Comment not found or you do not have permission to delete it."
                }
            );
        }

        return NoContent();
    }
}