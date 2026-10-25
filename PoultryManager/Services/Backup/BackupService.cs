using PoultryManager.Services.Api;

namespace PoultryManager.Services.Backup;

/// <summary>Implémentation de <see cref="IBackupService"/> : export JSON fourni par l'API.</summary>
public sealed class BackupService : IBackupService
{
    private readonly IApiClient _api;

    public BackupService(IApiClient api)
    {
        _api = api;
    }

    public Task<byte[]> CreateBackupAsync() => _api.GetBytesAsync("api/backup");
}
