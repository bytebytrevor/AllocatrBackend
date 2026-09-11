namespace AllocatrApi.Dtos;

public record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount
)
{
    public int TotalPages =>
        TotalCount == 0
            ? 0
            : (int)Math.Ceiling(
                TotalCount / (double)PageSize
            );
}