namespace AllocatrApi.Dtos;

public record class LocationCountryDto(
    string Code,
    string Name,
    bool SupportsAllocatOperations
);

public record class LocationCityDto(
    int Id,
    string Name,
    string CountryCode
);
