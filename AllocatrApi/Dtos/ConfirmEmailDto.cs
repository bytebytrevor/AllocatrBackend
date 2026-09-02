namespace AllocatrApi.Dtos;

public record ConfirmEmailDto(
    Guid UserId,
    string Token
);