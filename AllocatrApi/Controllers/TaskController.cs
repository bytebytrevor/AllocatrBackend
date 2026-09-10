using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/projects/tasks")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly UserManager<AllocatrUser> _userManager;
    private readonly TaskService _taskService;

    public TaskController(
        UserManager<AllocatrUser> userManager,
        TaskService taskService)
    {
        _userManager = userManager;
        _taskService = taskService;
    }

    [HttpGet("{projectId:guid}")]
    public async Task<IActionResult> GetTasksForProject(
        Guid projectId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var tasks = await _taskService.GetTasksByProjectIdAsync(
            projectId,
            user.Id,
            user.IsAllocat
        );

        if (tasks == null)
        {
            return NotFound(
                new { message = "Project not found." }
            );
        }

        return Ok(tasks);
    }

    [HttpGet("task/{taskId:guid}")]
    public async Task<IActionResult> GetTaskById(
        Guid taskId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var task = await _taskService.GetTaskByIdAsync(
            taskId,
            user.Id,
            user.IsAllocat
        );

        if (task == null)
        {
            return NotFound(
                new { message = "Task not found." }
            );
        }

        return Ok(task);
    }

    [HttpPost("{projectId:guid}")]
    public async Task<IActionResult> CreateTask(
        Guid projectId,
        [FromBody] CreateTaskDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        try
        {
            var task = await _taskService.CreateTaskAsync(
                projectId,
                user.Id,
                user.IsAllocat,
                dto
            );

            if (task == null)
            {
                return NotFound(
                    new { message = "Project not found." }
                );
            }

            return CreatedAtAction(
                nameof(GetTasksForProject),
                new { projectId },
                task
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new { message = ex.Message }
            );
        }
    }

    [HttpPatch("task/{taskId:guid}/status")]
    public async Task<IActionResult> UpdateTaskStatus(
        Guid taskId,
        [FromBody] UpdateTaskStatusDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _taskService.UpdateTaskStatusAsync(
                taskId,
                user.Id,
                user.IsAllocat,
                dto.Status
            );

            if (result == null)
            {
                return NotFound(
                    new { message = "Task not found." }
                );
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new { message = ex.Message }
            );
        }
    }

    [HttpDelete("task/{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(
        Guid taskId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var deleted = await _taskService.DeleteTaskAsync(
            taskId,
            user.Id,
            user.IsAllocat
        );

        if (!deleted)
        {
            return NotFound(
                new { message = "Task not found." }
            );
        }

        return NoContent();
    }
}