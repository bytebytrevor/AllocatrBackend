namespace AllocatrApi.Services.Storage;

public interface IFileStorageService
{
    Task UploadAsync(
        string bucket,
        string path,
        byte[] bytes,
        bool upsert = false
    );

    Task DeleteAsync(
        string bucket,
        string path
    );

    string GetPublicUrl(
        string bucket,
        string path
    );

    Task<string> CreateSignedUrlAsync(
        string bucket,
        string path,
        int expiresInSeconds = 300
    );
}