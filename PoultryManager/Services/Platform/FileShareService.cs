using PoultryManager.Services.Sharing;

namespace PoultryManager.Services.Platform;

/// <summary>Implémentation MAUI de <see cref="IFileShareService"/> (FileSystem + Share).</summary>
public sealed class FileShareService : IFileShareService
{
    public async Task ShareAsync(string fileName, byte[] content, string contentType, string title)
    {
        var path = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllBytesAsync(path, content);
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = title,
            File = new ShareFile(path, contentType)
        });
    }

    public async Task ShareCopyAsync(string sourcePath, string fileName, string contentType, string title)
    {
        var path = Path.Combine(FileSystem.CacheDirectory, fileName);
        await using (var source = File.Open(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        await using (var target = File.Create(path))
        {
            await source.CopyToAsync(target);
        }

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = title,
            File = new ShareFile(path, contentType)
        });
    }
}
