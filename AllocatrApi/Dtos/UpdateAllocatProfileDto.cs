namespace AllocatrApi.Dtos;

public class UpdateAllocatProfileDto
{
    public string? IdNumber { get; set; }

    public string? Title { get; set; }
    public string? Headline { get; set; }
    public string? Bio { get; set; }

    public decimal? HourlyRate { get; set; }
    public string? Currency { get; set; }

    public string? Availability { get; set; }
    public int? YearsExperience { get; set; }

    public List<Guid> SkillIds { get; set; } = [];
}