using Microsoft.Extensions.Caching.Memory;

namespace AllocatrApi.Services;

// Dedicated bounded cache. Originals are never retained here.
public sealed class ProjectThumbnailCache : IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 64L * 1024 * 1024 });
    private readonly SemaphoreSlim _generation = new(2, 2);
    private readonly SemaphoreSlim[] _locks = Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1)).ToArray();

    public async Task<byte[]> GetAsync(string key, Func<CancellationToken, Task<byte[]>> factory,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<byte[]>(key, out var ready)) return ready!;
        var gate = _locks[(key.GetHashCode() & int.MaxValue) % _locks.Length];
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue<byte[]>(key, out ready)) return ready!;
            await _generation.WaitAsync(cancellationToken);
            try
            {
                var bytes = await factory(cancellationToken);
                _cache.Set(key, bytes, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12),
                    Size = bytes.LongLength,
                });
                return bytes;
            }
            finally { _generation.Release(); }
        }
        finally { gate.Release(); }
    }

    public void Dispose()
    {
        _cache.Dispose();
        _generation.Dispose();
        foreach (var gate in _locks) gate.Dispose();
    }
}
