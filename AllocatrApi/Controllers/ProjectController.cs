using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AllocatrApi.Constants;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly UserManager<AllocatrUser> _userManager;
    private readonly AllocatrDbContext _db;
    private readonly ProjectService _projectService;
    private readonly ProjectAccessService _projectAccessService;

    public ProjectController(
        UserManager<AllocatrUser> userManager,
        AllocatrDbContext db,
        ProjectService projectService,
        ProjectAccessService projectAccessService)
    {
        _userManager = userManager;
        _db = db;
        _projectService = projectService;
        _projectAccessService = projectAccessService;
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

        var validSkillIds = await _db.Skills
            .Where(s => dto.SkillIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        if (validSkillIds.Count != dto.SkillIds.Count)
        {
            return BadRequest(new
            {
                message = "One or more selected skills are invalid."
            });
        }

        var project = new Project
        {
            ProjectCode = GenerateProjectCode(),
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Category = dto.Category.Trim(),

            ProjectSkills = dto.SkillIds
                .Select(skillId => new ProjectSkill
                {
                    SkillId = skillId
                })
                .ToList(),

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            StartDate = dto.StartDate,
            DueDate = dto.DueDate,

            Status = ProjectStatuses.Pending,
            Progress = 0,
            Priority = dto.Priority,

            UserId = user.Id,

            IsPublic = dto.IsPublic,
            AllowBids = dto.AllowBids,

            Budget = dto.Budget,
            Currency = dto.Currency
        };

        _db.Projects.Add(project);

        await _db.SaveChangesAsync();

        var result = await _projectService
            .GetAccessibleProjectByIdAsync(
                project.Id,
                user.Id,
                user.IsAllocat
            );

        if (result == null)
        {
            return StatusCode(500, new
            {
                message = "Project was created but could not be loaded."
            });
        }

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
    COMPLETE PROJECT
    ===================================================== */

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> CompleteProject(
        Guid id)
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var project =
            await _projectService
                .CompleteOwnedProjectAsync(
                    id,
                    user.Id
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
    REQUEST COMPLETION
    ===================================================== */

    [HttpPatch("{id:guid}/completion/request")]
    public async Task<IActionResult> RequestCompletion(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var project = await _projectService.RequestCompletionAsync(
            id,
            user.Id
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
    CONFIRM COMPLETION
    ===================================================== */

    [HttpPatch("{id:guid}/completion/confirm")]
    public async Task<IActionResult> ConfirmCompletion(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var project = await _projectService.ConfirmCompletionAsync(
            id,
            user.Id
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
    NEEDS MORE WORK
    ===================================================== */

    [HttpPatch("{id:guid}/completion/reject")]
    public async Task<IActionResult> RejectCompletion(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var project = await _projectService.NeedsMoreWorkAsync(
            id,
            user.Id
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
       PROJECT PERMISSIONS
    ===================================================== */

    [HttpGet("{id:guid}/permissions")]
    public async Task<IActionResult> GetProjectPermissions(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var canView = await _projectAccessService.CanViewProjectAsync(
            id,
            user.Id,
            user.IsAllocat
        );

        if (!canView)
        {
            return NotFound(new { message = "Project not found." });
        }

        var isOwner = await _projectAccessService.IsProjectOwnerAsync(
            id,
            user.Id
        );

        var isAcceptedAllocat =
            user.IsAllocat &&
            await _projectAccessService.IsAcceptedAllocatAsync(
                id,
                user.Id
            );

        return Ok(
            new ProjectPermissionsDto(
                isOwner,
                isAcceptedAllocat,
                isAcceptedAllocat
            )
        );
    }

    /* =====================================================
       HELPERS
    ===================================================== */

    private static string GenerateProjectCode()
    {
        return $"PRJ-{Guid.NewGuid():N}".ToUpper();
    }
}