using System.Text.Json.Serialization;

namespace AllocatrApi.Dtos;

public class UpdateAllocatProfileDto
{
    public string? IdNumber { get; set; }

    public string? Title { get; set; }
    public string? Headline { get; set; }
    public string? Bio { get; set; }

    private string? _operatingCountryCode;
    private int? _operatingCityId;

    // Track omission separately from explicit null for older edit requests.
    public string? OperatingCountryCode
    {
        get => _operatingCountryCode;
        set
        {
            _operatingCountryCode = value;
            OperatingCountryCodeProvided = true;
        }
    }

    public int? OperatingCityId
    {
        get => _operatingCityId;
        set
        {
            _operatingCityId = value;
            OperatingCityIdProvided = true;
        }
    }

    [JsonIgnore]
    public bool OperatingCountryCodeProvided { get; private set; }

    [JsonIgnore]
    public bool OperatingCityIdProvided { get; private set; }

    public decimal? HourlyRate { get; set; }
    public string? Currency { get; set; }

    public string? Availability { get; set; }
    public int? YearsExperience { get; set; }

    public List<Guid> SkillIds { get; set; } = [];
}
