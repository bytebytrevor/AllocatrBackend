// using AllocatrApi.Enums;

// namespace AllocatrApi.Dtos;

// public record class AllocatWorkProjectDto(
//     Guid Id,
//     string Title,
//     string Description,
//     string Status,
//     int Progress,
//     string? Priority,
//     DateOnly? StartDate,
//     DateOnly? DueDate,
//     decimal? Budget,
//     string Currency,
//     bool HasAcceptedAllocat,
//     ProjectAllocatStatus ProjectAllocatStatus,
//     DateTime InvitedAt,
//     DateTime? RespondedAt
// );

using AllocatrApi.Enums;

namespace AllocatrApi.Dtos;

public record AllocatWorkProjectDto(
    Guid Id,
    string ProjectCode,
    string Title,
    string Description,
    string Category,
    string Status,
    int Progress,
    string? Priority,
    DateOnly? StartDate,
    DateOnly? DueDate,
    decimal? Budget,
    string Currency,
    bool HasAcceptedAllocat,
    DateTime CreatedAt,
    ProjectAllocatStatus ProjectAllocatStatus,
    DateTime InvitedAt,
    DateTime? RespondedAt
);