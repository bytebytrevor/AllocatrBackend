using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AllocatrApi.Services;
using AllocatrApi.Models;
using Microsoft.AspNetCore.Identity;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/skill-categories")]
[Authorize]
public class SkillCategoryController : ControllerBase
{
    private readonly SkillCategoryService _skillCategoryService;

    public SkillCategoryController(SkillCategoryService skillCategoryService)
    {
        _skillCategoryService = skillCategoryService;
    }

    // Post: api/skill-categories
    // [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateSkillCategory(CreateSkillCategoryDto dto)
    {
        var result = await _skillCategoryService.CreateSkillCategoryAsync(dto);

        return CreatedAtAction(
            nameof(GetSkillCategoryById),
            new { id = result.Id },
            result
        );
    }

    // Get: api/skill-categories/{id}
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSkillCategoryById(Guid id)
    {
        var result = await _skillCategoryService.GetSkillCategoryByIdAsync(id);

        if (result == null)
            return NotFound("Skill category not found");

        return Ok(result);
    }

    // Get: api/skill-categories
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllSkillCategories()
    {
        var result = await _skillCategoryService.GetAllSkillCategoriesAsync();
        return Ok(result);
    }
}