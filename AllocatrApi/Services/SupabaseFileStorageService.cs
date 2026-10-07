namespace AllocatrApi.Services.Storage;

public class SupabaseFileStorageService : IFileStorageService
{
    private readonly SupabaseService _supabase;

    public SupabaseFileStorageService(
        SupabaseService supabase
    )
    {
        _supabase = supabase;
    }

    public async Task UploadAsync(
        string bucket,
        string path,
        byte[] bytes,
        bool upsert = false
    )
    {
        await _supabase.Client
            .Storage
            .From(bucket)
            .Upload(
                bytes,
                path,
                new Supabase.Storage.FileOptions
                {
                    Upsert = upsert
                }
            );
    }

    public async Task DeleteAsync(
        string bucket,
        string path
    )
    {
        await _supabase.Client
            .Storage
            .From(bucket)
            .Remove(
                new List<string>
                {
                    path
                }
            );
    }

    public string GetPublicUrl(
        string bucket,
        string path
    )
    {
        return _supabase.Client
            .Storage
            .From(bucket)
            .GetPublicUrl(path);
    }

    public async Task<string> CreateSignedUrlAsync(
        string bucket,
        string path,
        int expiresInSeconds = 300
    )
    {
        return await _supabase.Client
            .Storage
            .From(bucket)
            .CreateSignedUrl(
                path,
                expiresInSeconds
            );
    }
}