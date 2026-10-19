using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace AllocatrApi.Services;

public sealed class ProjectThumbnailOptions
{
    public string FfmpegPath { get; set; } = "ffmpeg";
    public int TimeoutSeconds { get; set; } = 20;
}

public sealed class ProjectThumbnailEncoder : IDisposable
{
    private readonly ProjectThumbnailOptions _options;
    private readonly SemaphoreSlim _workers = new(2, 2);
    public ProjectThumbnailEncoder(IOptions<ProjectThumbnailOptions> options) => _options = options.Value;

    public async Task<byte[]> GenerateAsync(byte[] original, string contentType, CancellationToken cancellationToken)
    {
        await _workers.WaitAsync(cancellationToken);
        var directory = Path.Combine(Path.GetTempPath(), "allocatr-thumbnails", Guid.NewGuid().ToString("N"));
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(_options.TimeoutSeconds, 5, 60)));
            var format = contentType switch
            {
                "image/jpeg" => "jpeg_pipe", "image/png" => "png_pipe", "image/webp" => "webp_pipe",
                "video/mp4" => "mov", "video/webm" => "matroska",
                _ => throw new ProjectFileException(415, "This file does not have a thumbnail."),
            };
            Directory.CreateDirectory(directory);
            var input = Path.Combine(directory, "input.media");
            var output = Path.Combine(directory, "thumbnail.jpg");
            await File.WriteAllBytesAsync(input, original, timeout.Token);
            var start = new ProcessStartInfo(_options.FfmpegPath)
            {
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardError = true, WorkingDirectory = directory,
            };
            // Controlled arguments and local temporary files only; no shell or remote decoder URLs.
            foreach (var argument in new[] { "-hide_banner", "-loglevel", "error", "-nostdin", "-y",
                "-max_alloc", "67108864", "-threads", "1", "-filter_threads", "1",
                "-protocol_whitelist", "file,pipe", "-f", format }) start.ArgumentList.Add(argument);
            if (contentType == "video/mp4") { start.ArgumentList.Add("-enable_drefs"); start.ArgumentList.Add("0"); }
            foreach (var argument in new[] { "-i", input, "-map", "0:v:0", "-an", "-sn", "-dn",
                "-frames:v", "1", "-vf", "scale=w='min(640,iw)':h='min(360,ih)':force_original_aspect_ratio=decrease:force_divisible_by=2,setsar=1",
                "-c:v", "mjpeg", "-q:v", "6", "-pix_fmt", "yuvj420p", "-threads", "1",
                "-update", "1", "-f", "image2", output }) start.ArgumentList.Add(argument);
            using var process = new Process { StartInfo = start };
            if (!process.Start()) throw new IOException("Thumbnail encoder did not start.");
            var drain = DrainAsync(process.StandardError);
            try { await process.WaitForExitAsync(timeout.Token); }
            catch
            {
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
                await process.WaitForExitAsync(CancellationToken.None);
                await drain;
                throw;
            }
            await drain;
            if (process.ExitCode != 0 || !File.Exists(output)) throw new IOException("Thumbnail encoding failed.");
            var length = new FileInfo(output).Length;
            if (length <= 0 || length > 256 * 1024) throw new IOException("Thumbnail size is invalid.");
            var bytes = await File.ReadAllBytesAsync(output, timeout.Token);
            if (bytes.Length < 3 || bytes[0] != 0xff || bytes[1] != 0xd8 || bytes[2] != 0xff)
                throw new IOException("Thumbnail format is invalid.");
            return bytes;
        }
        finally
        {
            try { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            _workers.Release();
        }
    }

    private static async Task DrainAsync(StreamReader reader)
    {
        var buffer = new char[2048];
        // Drain without retaining decoder output (which may contain untrusted media metadata).
        while (await reader.ReadAsync(buffer.AsMemory()) > 0) { }
    }
    public void Dispose() => _workers.Dispose();
}
