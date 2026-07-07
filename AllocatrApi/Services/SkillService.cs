using AllocatrApi.Data;
namespace AllocatrApi.Services;

public class SkillService
{
    private readonly AllocatrDbContext _db;

    public SkillService(AllocatrDbCOntext db)
    {
        _db = db;
    }

    public async Task<SkillDto> CreateSkillAsync(CreateSkillDto dto)
    {
        var skill = new SkillDto(

        );
    }
    
}