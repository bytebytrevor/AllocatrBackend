namespace AllocatrApi.Dtos;

public record class ProjectMemberDto(
    string UserId,
    string Role,
    bool IsOwner,
    DateTime JoinedAt
);
