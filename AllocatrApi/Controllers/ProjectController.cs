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

    /* =====================================================
       GET ACCESSIBLE PROJECTS
    ===================================================== */

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var projects = await _projectService.GetAccessibleProjectsAsync(
            user.Id,
            user.IsAllocat
        );

        return Ok(projects);
    }

    /* =====================================================
       GET PROJECT
    ===================================================== */

    [HttpGet("{id:guid}", Name = "GetProjectById")]
    public async Task<IActionResult> GetProjectById(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var project = await _projectService.GetAccessibleProjectByIdAsync(
            id,
            user.Id,
            user.IsAllocat
        );

        if (project == null)
        {
            return NotFound(new
            {
                message = "Project not found."
            });
        }

        return Ok(project);
    }

    /* =====================================================
       GET OWN PROJECTS
    ===================================================== */

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyProjects()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var projects = await _projectService.GetProjectsByUserAsync(
            user.Id
        );

        return Ok(projects);
    }

    /* =====================================================
       CREATE PROJECT
    ===================================================== */

    [HttpPost]
    public async Task<IActionResult> CreateProject(CreateProjectDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var project = new Project
        {
            ProjectCode = GenerateProjectCode(),
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,

            Tags = dto.Tags
                .Select(tag => new ProjectTag
                {
                    Tag = tag
                })
                .ToList(),

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
            false,
            project.CreatedAt,
            project.StartDate,
            project.DueDate,
            project.AllocatAssignments
        );

        return CreatedAtAction(
            nameof(GetProjectById),
            new
            {
                id = project.Id
            },
            result
        );
    }

    /* =====================================================
       UPDATE PROJECT
    ===================================================== */

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateProject(
        Guid id,
        UpdateProjectDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        try
        {
            var project = await _projectService.UpdateOwnedProjectAsync(
                id,
                user.Id,
                dto
            );

            if (project == null)
            {
                return NotFound(new
                {
                    message = "Project not found."
                });
            }

            return Ok(project);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    /* =====================================================
       HELPERS
    ===================================================== */

    private static string GenerateProjectCode()
    {
        return $"PRJ-{Guid.NewGuid():N}".ToUpper();
    }
}