using System.Net.Http;
using AllocatrApi.Services.Storage;
using Microsoft.Extensions.Logging;

namespace AllocatrApi.Services;

public sealed record ProjectThumbnailSource(string Bucket, string StoragePath, string ContentType);

public sealed class ProjectThumbnailService
{
    private readonly ProjectFileService _files;
    private readonly IFileStorageService _storage;
    private readonly IHttpClientFactory _clients;
    private readonly ProjectThumbnailCache _cache;
    private readonly ProjectThumbnailEncoder _encoder;
    private readonly ILogger<ProjectThumbnailService> _logger;

    public ProjectThumbnailService(ProjectFileService files, IFileStorageService storage, IHttpClientFactory clients,
        ProjectThumbnailCache cache, ProjectThumbnailEncoder encoder, ILogger<ProjectThumbnailService> logger)
    { _files = files; _storage = storage; _clients = clients; _cache = cache; _encoder = encoder; _logger = logger; }

    public async Task<byte[]> GetAsync(Guid projectId, Guid fileId, Guid userId, bool isAllocat, CancellationToken cancellationToken)
    {
        // Recheck membership and Ready status BEFORE serving even a server-cache hit.
        var source = await _files.GetThumbnailSourceAsync(projectId, fileId, userId, isAllocat, cancellationToken);
        try
        {
            var result = await _cache.GetAsync($"thumbnail-v1/{projectId:N}/{fileId:N}", async token =>
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
                timeout.CancelAfter(TimeSpan.FromSeconds(60));
                var workToken = timeout.Token;
                var signed = await _storage.CreateSignedUrlAsync(source.Bucket, source.StoragePath, 60);
                if (!Uri.TryCreate(signed, UriKind.Absolute, out var url) || (url.Scheme != "https" && url.Scheme != "http"))
                    throw new IOException("Invalid storage response.");
                using var response = await _clients.CreateClient("ProjectThumbnailSource")
                    .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, workToken);
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentLength > ProjectFileValidationService.MaxFileSizeBytes)
                    throw new IOException("Source exceeds the supported size.");
                await using var input = await response.Content.ReadAsStreamAsync(workToken);
                await using var output = new MemoryStream();
                var buffer = new byte[81920];
                int read;
                while ((read = await input.ReadAsync(buffer.AsMemory(), workToken)) > 0)
                {
                    if (output.Length + read > ProjectFileValidationService.MaxFileSizeBytes) throw new IOException("Source exceeds the supported size.");
                    await output.WriteAsync(buffer.AsMemory(0, read), workToken);
                }
                return await _encoder.GenerateAsync(output.ToArray(), source.ContentType, workToken);
            }, cancellationToken);
            // Permission/removal may have changed while a cache miss was being generated.
            await _files.GetThumbnailSourceAsync(projectId, fileId, userId, isAllocat, cancellationToken);
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (ProjectFileException) { throw; }
        catch (Exception exception)
        {
            // Do not log signed bearer URLs or decoder output.
            _logger.LogWarning("Thumbnail unavailable for project file {ProjectFileId}; failure type {FailureType}", fileId, exception.GetType().Name);
            throw new ProjectFileException(503, "This thumbnail is temporarily unavailable.");
        }
    }
}
