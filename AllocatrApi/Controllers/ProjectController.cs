using System;
using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly UserManager<AllocatrUser> _userManager;
    private readonly AllocatrDbContext _db;
    private readonly ProjectService _projectService;

    public ProjectController(
        UserManager<AllocatrUser> userManager,
        AllocatrDbContext db,
        ProjectService projectService)
    {
        _userManager = userManager;
        _db = db;
        _projectService = projectService;
    }

    // GET api/projects/id
    [HttpGet("{id}", Name = "GetProjectById")]
    public async Task<IActionResult> GetProjectById(string id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null)
            return NotFound("Project not found");

        return Ok(new ProjectDto(
            project.Id,
            project.ProjectCode,
            project.Title,
            project.Description,
            project.Category,
            project.Status,
            project.Progress,
            project.Priority,
            project.Budget,
            project.Currency,
            project.StartDate,
            project.DueDate,
            project.AllocatAssignments
        ));
    }

    // GET api/projects/mine
    [HttpGet("mine")]
    public async Task<IActionResult> GetProjectsByUserId()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var projects = await _projectService.GetProjectsByUserAsync(user.Id);
        return Ok(projects);
    }

    // GET api/projects
    [HttpGet("")]
    public async Task<IActionResult> GetAllProjects()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var projects = await _projectService.GetAllProjectsAsync();
        return Ok(projects);
    }

    // POST api/projects/
    [HttpPost("")]
    public async Task<IActionResult> CreateProject(CreateProjectDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var project = new Project
        {
            ProjectCode = GenerateProjectCode(),
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Tags = dto.Tags.Select(t => new ProjectTag { Tag = t }).ToList(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            StartDate = dto.StartDate,
            DueDate = dto.DueDate,
            Status = "pending",
            Progress = 0,
            Priority = dto.Priority,
            UserId = user.Id,
            IsPublic = true,
            AllowBids = true,
            Budget = dto.Budget,
            Currency = dto.Currency
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var result = new ProjectDto(
            project.Id,
            project.ProjectCode,
            project.Title,
            project.Description,
            project.Category,
            project.Status,
            project.Progress,
            project.Priority,
            project.Budget,
            project.Currency,
            project.StartDate,
            project.DueDate,
            project.AllocatAssignments
        );


        return CreatedAtAction(
            nameof(GetProjectById),
            new { id = project.Id },
            result
        );
    }

    public string GenerateProjectCode()
    {
        return "ALC00012026";
    }
}
