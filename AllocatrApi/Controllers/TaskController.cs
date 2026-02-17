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

    // GET api/projects/tasks/{projectId}
    [HttpGet("{projectId:guid}")]
    public async Task<IActionResult> GetTasksForProject(Guid projectId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var tasks = await _taskService.GetTasksByProjectAsync(projectId);
        return Ok(tasks);
    }

    // GET api/projects/tasks/task/{taskId}
    [HttpGet("task/{taskId:guid}")]
    public async Task<IActionResult> GetTaskById(Guid taskId)
    {
        var task = await _taskService.GetTaskByIdAsync(taskId);
        if (task == null)
            return NotFound();

        return Ok(task);
    }

    // GET api/projects/tasks
    [HttpGet]
    public async Task<IActionResult> GetAllTasks()
    {
        var tasks = await _taskService.GetAllTasksAsync();
        return Ok(tasks);
    }

    // POST api/projects/tasks/{projectId}
    [HttpPost("{projectId:guid}")]
    public async Task<IActionResult> CreateTask(
        Guid projectId,
        [FromBody] CreateTaskDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = "pending",
            DueDate = dto.DueDate,
            AssignedToId = dto.AssignedToId,
            CreatedByUserId = user.Id
        };

        var createdTask = await _taskService.CreateTaskAsync(task);

        var newDto = new TaskDto(
            createdTask.Id,
            createdTask.Title,
            createdTask.Description,
            createdTask.Status,
            createdTask.Priority,
            createdTask.DueDate
        );

        return CreatedAtAction(nameof(GetTasksForProject), new { projectId }, newDto);
    }
}