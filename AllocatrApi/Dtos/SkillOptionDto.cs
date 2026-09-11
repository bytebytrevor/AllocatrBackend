namespace AllocatrApi.Dtos;

public record SkillOptionDto(
    Guid Id,
    string Name,
    Guid CategoryId,
    string Category
);