using System.IO.Compression;
using System.Buffers.Binary;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace AllocatrApi.Services;

public sealed record ValidatedProjectUpload(
    string OriginalFileName, string ContentType, string Extension, byte[] Bytes);

public class ProjectFileValidationService
{
    public const long MaxFileSizeBytes = 25L * 1024 * 1024;
    public const long MaxRequestSizeBytes = MaxFileSizeBytes + 1024 * 1024;
    private const int MaxArchiveEntries = 2000;
    private const long MaxExpandedArchiveBytes = 100L * 1024 * 1024;

    private static readonly Dictionary<string, string[]> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = ["application/pdf"],
        [".jpg"] = ["image/jpeg"],
        [".jpeg"] = ["image/jpeg"],
        [".png"] = ["image/png"],
        [".webp"] = ["image/webp"],
        [".doc"] = ["application/msword"],
        [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
        [".xls"] = ["application/vnd.ms-excel"],
        [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
        [".csv"] = ["text/csv", "application/csv", "application/vnd.ms-excel", "text/plain"],
        [".txt"] = ["text/plain"],
        [".mp4"] = ["video/mp4"],
        [".webm"] = ["video/webm"],
        [".zip"] = ["application/zip", "application/x-zip-compressed"],
    };

    private static readonly HashSet<string> UnsafeArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".com", ".scr", ".msi", ".msp", ".bat", ".cmd", ".ps1",
        ".psm1", ".vbs", ".vbe", ".js", ".jse", ".wsf", ".wsh", ".hta", ".lnk",
        ".url", ".html", ".htm", ".svg", ".sh", ".jar", ".apk", ".app", ".php",
        ".aspx", ".asp", ".docm", ".xlsm", ".pptm", ".zip", ".7z", ".rar",
        ".gz", ".tar", ".iso", ".img", ".cab", ".doc", ".xls",
    };

    public async Task<ValidatedProjectUpload> ValidateAndReadAsync(
        IFormFile? file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length <= 0)
            throw new ProjectFileException(400, "Choose a non-empty file to upload.");
        if (file.Length > MaxFileSizeBytes)
            throw new ProjectFileException(413, "Project files cannot exceed 25 MB.");

        var name = CleanFileName(file.FileName);
        var extension = Path.GetExtension(name).ToLowerInvariant();
        var mime = (file.ContentType ?? string.Empty).Split(';', 2)[0].Trim().ToLowerInvariant();
        if (!Allowed.TryGetValue(extension, out var types) || !types.Contains(mime))
            throw new ProjectFileException(415,
                "Use PDF, JPEG, PNG, WebP, DOC, DOCX, XLS, XLSX, CSV, TXT, ZIP, MP4 or WebM with a matching content type.");

        // Bound the actual stream as well as the client-supplied length.
        await using var source = file.OpenReadStream();
        await using var output = new MemoryStream();
        var buffer = new byte[81920];
        int read;
        while ((read = await source.ReadAsync(buffer.AsMemory(), cancellationToken)) > 0)
        {
            if (output.Length + read > MaxFileSizeBytes)
                throw new ProjectFileException(413, "Project files cannot exceed 25 MB.");
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
        var bytes = output.ToArray();
        if (bytes.LongLength != file.Length || bytes.Length == 0)
            throw new ProjectFileException(400, "The uploaded file size is invalid.");
        ValidateContents(extension, bytes);
        return new ValidatedProjectUpload(name, types[0], extension, bytes);
    }

    private static string CleanFileName(string supplied)
    {
        // Handle Windows and Unix client paths independently of the API OS.
        var leaf = (supplied ?? string.Empty).Replace('\\', '/').Split('/').Last();
        var name = new string(leaf.Where(character =>
            !char.IsControl(character) && character is not '\u202a' and not '\u202b'
                and not '\u202c' and not '\u202d' and not '\u202e'
                and not '\u2066' and not '\u2067' and not '\u2068' and not '\u2069')
            .ToArray()).Trim();
        if (name.Length == 0 || name.Length > 255)
            throw new ProjectFileException(400, "The filename must contain between 1 and 255 characters.");
        return name;
    }

    private static void ValidateContents(string extension, byte[] bytes)
    {
        bool valid = extension switch
        {
            ".pdf" => bytes.AsSpan().StartsWith("%PDF-"u8),
            ".jpg" or ".jpeg" => bytes.AsSpan().StartsWith(new byte[] { 0xff, 0xd8, 0xff }),
            ".png" => bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".webp" => bytes.Length >= 12 && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            ".doc" or ".xls" => bytes.AsSpan().StartsWith(
                new byte[] { 0xd0, 0xcf, 0x11, 0xe0, 0xa1, 0xb1, 0x1a, 0xe1 }),
            ".docx" or ".xlsx" or ".zip" => ValidateArchive(extension, bytes),
            ".txt" or ".csv" => IsText(bytes),
            ".mp4" => IsMp4(bytes),
            ".webm" => IsWebM(bytes),
            _ => false,
        };
        if (!valid)
            throw new ProjectFileException(415, "The file contents do not match a supported file format.");
    }

    // Container signature checks, consistent with other allowed media formats.
    // These do not decode/transcode or guarantee browser codec support.
    private static bool IsMp4(byte[] bytes)
    {
        if (bytes.Length < 16 || !bytes.AsSpan(4, 4).SequenceEqual("ftyp"u8)) return false;
        var size = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(0, 4));
        if (size < 16 || size > bytes.Length || (size - 16) % 4 != 0) return false;
        var brand = Encoding.ASCII.GetString(bytes, 8, 4);
        return brand is "isom" or "iso2" or "iso3" or "iso4" or "iso5" or "iso6"
            or "iso7" or "iso8" or "iso9" or "mp41" or "mp42" or "avc1" or "M4V " or "dash";
    }

    private static bool IsWebM(byte[] bytes)
    {
        if (!bytes.AsSpan().StartsWith(new byte[] { 0x1a, 0x45, 0xdf, 0xa3 })) return false;
        var offset = 4;
        if (!ReadEbmlInteger(bytes, ref offset, false, out var length)
            || length > 4096 || length > (ulong)(bytes.Length - offset)) return false;
        var end = offset + (int)length;
        while (offset < end)
        {
            if (!ReadEbmlInteger(bytes.AsSpan(0, end), ref offset, true, out var id)
                || !ReadEbmlInteger(bytes.AsSpan(0, end), ref offset, false, out var size)
                || size > (ulong)(end - offset)) return false;
            if (id == 0x4282) return size == 4 && bytes.AsSpan(offset, 4).SequenceEqual("webm"u8);
            offset += (int)size;
        }
        return false;
    }

    private static bool ReadEbmlInteger(ReadOnlySpan<byte> bytes, ref int offset,
        bool keepMarker, out ulong value)
    {
        value = 0;
        if (offset >= bytes.Length || bytes[offset] == 0) return false;
        var marker = 0x80;
        var length = 1;
        while ((bytes[offset] & marker) == 0) { marker >>= 1; length++; }
        if (length > (keepMarker ? 4 : 8) || length > bytes.Length - offset) return false;
        value = keepMarker ? bytes[offset] : (ulong)(bytes[offset] & (marker - 1));
        for (var i = 1; i < length; i++) value = (value << 8) | bytes[offset + i];
        offset += length;
        return true;
    }

    private static bool IsText(byte[] bytes)
    {
        if (bytes.Contains((byte)0)) return false;
        try { _ = new UTF8Encoding(false, true).GetString(bytes); return true; }
        catch (DecoderFallbackException) { return false; }
    }

    private static bool ValidateArchive(string extension, byte[] bytes)
    {
        try
        {
            using var stream = new MemoryStream(bytes, writable: false);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            if (archive.Entries.Count == 0 || archive.Entries.Count > MaxArchiveEntries) return false;
            long total = 0;
            foreach (var entry in archive.Entries)
            {
                var name = entry.FullName.Replace('\\', '/');
                if (name.StartsWith('/') || name.Contains(':') || name.Split('/').Contains("..")) return false;
                if (entry.Length > MaxExpandedArchiveBytes - total) return false;
                total += entry.Length;
                if (UnsafeArchiveExtensions.Contains(Path.GetExtension(name))) return false;
                if (name.EndsWith("vbaProject.bin", StringComparison.OrdinalIgnoreCase)) return false;
            }
            if (extension == ".zip") return true;
            var documentEntry = extension == ".docx" ? "word/document.xml" : "xl/workbook.xml";
            return archive.GetEntry("[Content_Types].xml") != null
                && archive.GetEntry(documentEntry) != null;
        }
        catch (InvalidDataException) { return false; }
        catch (IOException) { return false; }
    }
}
