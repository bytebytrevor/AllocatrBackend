namespace AllocatrApi.Dtos;

public record class AllocatProjectSummaryDto(
    Guid Id,
    string ProjectCode,
    string Title,
    string Category,
    string Status
);