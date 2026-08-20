using AllocatrApi.Enums;

namespace AllocatrApi.Dtos;

public record class ProjectAllocatMemberDto(
    Guid AllocatProfileId,
    string FullName,
    string? AvatarUrl,
    ProjectAllocatStatus Status,
    DateTime InvitedAt,
    DateTime? RespondedAt
);