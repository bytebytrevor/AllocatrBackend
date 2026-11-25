using AllocatrApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AllocatrApi.Services;

// All project/work membership mutations take this lock BEFORE reading mutable state.
// Reuses a controller transaction; otherwise owns one until CommitAsync/disposal.
public sealed class ProjectMutationScope : IAsyncDisposable
{
    private IDbContextTransaction? _owned;

    private ProjectMutationScope(IDbContextTransaction? owned) => _owned = owned;

    public static async Task<ProjectMutationScope> BeginAsync(
        AllocatrDbContext db,
        Guid projectId,
        CancellationToken ct = default
    )
    {
        var owned =
            db.Database.CurrentTransaction == null
                ? await db.Database.BeginTransactionAsync(ct)
                : null;
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM \"Projects\" WHERE \"Id\" = {projectId} FOR UPDATE",
                ct
            );
            return new ProjectMutationScope(owned);
        }
        catch
        {
            if (owned != null)
                await owned.DisposeAsync();
            throw;
        }
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_owned == null)
            return;
        await _owned.CommitAsync(ct);
        await _owned.DisposeAsync();
        _owned = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_owned != null)
            await _owned.DisposeAsync();
    }
}
