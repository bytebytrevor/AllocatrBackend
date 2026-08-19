using AllocatrApi.Enums;

namespace AllocatrApi.Dtos;

public record class ProjectAllocatMemberDto(
    Guid AllocatProfileId,
    string FullName,
    string? AvatarUrl,
    // string? Title,
    ProjectAllocatStatus Status,
    DateTime InvitedAt,
    DateTime? RespondedAt
);