namespace AllocatrApi.Dtos;

public record class ProfileDto(
    Guid Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? Location,
    string? AvatarUrl,
    DateTime CreatedAt,
    bool EmailConfirmed,
    bool IsAllocat
);