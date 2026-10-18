using PoultryManager.Data;
using PoultryManager.Data.Repositories;
using PoultryManager.Services.Auth;
using PoultryManager.Services.Security;

namespace PoultryManager.Services.Startup;

/// <summary>Implémentation de <see cref="IAppInitializer"/>.</summary>
public sealed class AppInitializer : IAppInitializer
{
    private readonly IUserRepository _users;
    private readonly IBandRepository _bands;
    private readonly IRecordRepository _records;
    private readonly IAuthService _authService;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _seedChecked;

    public AppInitializer(IUserRepository users, IBandRepository bands, IRecordRepository records, IAuthService authService)
    {
        _users = users;
        _bands = bands;
        _records = records;
        _authService = authService;
    }

    public async Task<bool> InitializeAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!_seedChecked)
            {
                if (await _users.CountAsync() == 0)
                {
                    await SeedDemoDataAsync();
                }

                _seedChecked = true;
            }

            return _authService.IsAuthenticated || await _authService.TryRestoreSessionAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>Compte de démonstration et ses bandes (premier lancement uniquement).</summary>
    private async Task SeedDemoDataAsync()
    {
        var now = DateTime.Now;
        var user = DemoDataSeeder.CreateDemoUser(now);
        var (hash, salt) = await Task.Run(() => PasswordHasher.Hash(DemoDataSeeder.DemoPassword));
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        var userId = await _users.AddAsync(user);

        foreach (var demo in DemoDataSeeder.Bands)
        {
            var band = DemoDataSeeder.CreateBand(demo, userId, now);
            await _bands.AddAsync(band);
            var (eggs, feeds, mortality) = DemoDataSeeder.CreateRecords(band, demo.Seed, now);
            await _records.AddRangeAsync(eggs, feeds, mortality);
        }
    }
}
