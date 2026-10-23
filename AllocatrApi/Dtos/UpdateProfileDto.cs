using System.Text.Json.Serialization;

namespace AllocatrApi.Dtos;

public record class UpdateProfileDto(
    string FullName,
    string? PhoneNumber,
    string? Location = null
)
{
    private string? _countryCode;
    private string? _cityName;

    // The setters run when the JSON property is supplied, including explicit null.
    // Omission leaves the flag false, so older clients do not clear residence.
    public string? CountryCode
    {
        get => _countryCode;
        init
        {
            _countryCode = value;
            CountryCodeProvided = true;
        }
    }

    public string? CityName
    {
        get => _cityName;
        init
        {
            _cityName = value;
            CityNameProvided = true;
        }
    }

    [JsonIgnore]
    public bool CountryCodeProvided { get; private set; }

    [JsonIgnore]
    public bool CityNameProvided { get; private set; }
}
