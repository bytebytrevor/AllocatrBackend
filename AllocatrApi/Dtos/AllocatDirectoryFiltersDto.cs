namespace AllocatrApi.Dtos;

public record AllocatDirectoryFiltersDto(
    IReadOnlyList<SkillOptionDto> Skills,
    decimal MaxHourlyRate
);
