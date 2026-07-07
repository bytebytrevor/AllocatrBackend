namespace AllocatrApi.Controllers;

[ApiController]
[Authorize]
[Route("api/skills")]
public class SkillController : ControllerBase
{
    public async Task<IActionResult> CreateSkill(CreateSkillDto dto)
    {
        var result = _skillService.CreateSkillAsync(dto);
        
        return CreatedAtAction(
            nameof(GetSkillById),
            new { id = result.Id },
            result
        );
    }

} 