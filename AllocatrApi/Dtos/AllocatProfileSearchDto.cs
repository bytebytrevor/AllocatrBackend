namespace AllocatrApi.Dtos;

public class AllocatProfileSearchDto
{
    public string? Search { get; set; }
    public string? Location { get; set; }
    public Guid? SkillId { get; set; }

    public decimal? MaxHourlyRate { get; set; }
    public int? MinYearsExperience { get; set; }

    public bool? AvailableOnly { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 24;
}