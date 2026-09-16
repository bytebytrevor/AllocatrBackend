namespace AllocatrApi.Dtos;

public record ProjectSkillDto(
    Guid Id,
    string Name,
    Guid CategoryId,
    string Category
);