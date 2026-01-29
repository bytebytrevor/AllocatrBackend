using System;
using AllocatrApi.Data;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

public class TaskController : ControllerBase
{
    private readonly AllocatrDbContext _db;
    private readonly UserManager<AllocatrUser> _userManager;
    private readonly TaskService _taskService;

    TaskController(
        AllocatrDbContext db,
        UserManager<AllocatrUser> userManager,
        TaskService taskService)
    {
        _db = db;
        _userManager = userManager;
        _taskService = taskService;
    }

    public async Task<IActionResult> GetAllTasks()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var tasks = _taskService.GetAllTasksAsync();
        return Ok(tasks);
    }
}
