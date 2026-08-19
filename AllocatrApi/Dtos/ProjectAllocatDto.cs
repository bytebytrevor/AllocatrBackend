using AllocatrApi.Enums;

namespace AllocatrApi.Dtos;

public record class ProjectAllocatDto(
    Guid ProjectId,
    Guid AllocatProfileId,
    ProjectAllocatStatus Status,
    DateTime InvitedAt,
    DateTime? RespondedAt,
    DateTime? RemovededAt
);