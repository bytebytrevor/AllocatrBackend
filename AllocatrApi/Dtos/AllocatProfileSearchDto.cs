namespace AllocatrApi.Dtos;

public class AllocatProfileSearchDto
{
    public string? Search { get; set; }

    // Compatibility text filter; now searches operating location, never residence.
    public string? Location { get; set; }

    // Exact filters for location selectors. City IDs may be negative seeded IDs.
    public string? CountryCode { get; set; }
    public int? CityId { get; set; }

    public Guid? SkillId { get; set; }
    public decimal? MaxHourlyRate { get; set; }
    public int? MinYearsExperience { get; set; }
    public bool? AvailableOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 24;
}
