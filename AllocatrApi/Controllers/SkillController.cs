using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[ApiController]
[Authorize]
[Route("api/skills")]
public class SkillController : ControllerBase
{
    private readonly SkillService _skillService;

    public SkillController(SkillService skillService)
    {
        _skillService = skillService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSkill(CreateSkillDto dto)
    {
        var result = await _skillService.CreateSkillAsync(dto);

        if (result == null)
            return BadRequest("The selected skill category does not exist.");

        return CreatedAtAction(
            nameof(GetSkillById),
            new { id = result.Id },
            result
        );
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSkillById(Guid id)
    {
        var result = await _skillService.GetSkillByIdAsync(id);
        if (result == null)
            return NotFound("Skill not found.");

        return Ok(result);
    }
}