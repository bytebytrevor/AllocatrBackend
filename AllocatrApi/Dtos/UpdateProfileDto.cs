namespace AllocatrApi.Dtos;

public record class UpdateProfileDto(
    string FullName,
    string? PhoneNumber,
    string? Location
);