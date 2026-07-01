namespace AllocatrApi.Dtos;

public record class AllocatDto(
    Guid UserId,
    string FullName,
    bool IsAllocat
);
