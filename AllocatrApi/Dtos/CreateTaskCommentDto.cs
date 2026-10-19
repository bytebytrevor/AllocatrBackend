namespace AllocatrApi.Dtos;

public record CreateTaskCommentDto(
    string? Comment,
    IReadOnlyList<Guid>? FileIds = null,
    Guid? ClientRequestId = null);
