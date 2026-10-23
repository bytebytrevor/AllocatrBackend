namespace AllocatrApi.Dtos;

public class CreateAllocatProfileDto
{
    public string? IdNumber { get; set; }

    public string? Title { get; set; }
    public string? Headline { get; set; }
    public string? Bio { get; set; }

    // Required by service validation; these are not the user's residence fields.
    public string? OperatingCountryCode { get; set; }
    public int? OperatingCityId { get; set; }

    public decimal? HourlyRate { get; set; }
    public string? Currency { get; set; }

    public string? Availability { get; set; }
    public int? YearsExperience { get; set; }

    public List<Guid> SkillIds { get; set; } = [];
}
