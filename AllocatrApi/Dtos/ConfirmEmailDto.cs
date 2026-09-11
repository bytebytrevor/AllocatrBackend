namespace AllocatrApi.Dtos;

public record class ConfirmEmailDto(
    Guid UserId,
    string Token
);