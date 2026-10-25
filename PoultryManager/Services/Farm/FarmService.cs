using PoultryManager.Data.Repositories;
using PoultryManager.Models;
using PoultryManager.Services.Auth;

namespace PoultryManager.Services.Farm;

/// <summary>Implémentation de <see cref="IFarmService"/>.</summary>
public sealed class FarmService : IFarmService
{
    private readonly IAuthService _authService;
    private readonly IBandRepository _bands;
    private readonly IRecordRepository _records;

    public FarmService(IAuthService authService, IBandRepository bands, IRecordRepository records)
    {
        _authService = authService;
        _bands = bands;
        _records = records;
    }

    public async Task<IReadOnlyList<Band>> GetBandsAsync(bool includeClosed = true)
    {
        var user = _authService.CurrentUser;
        if (user is null)
        {
            return [];
        }

        var bands = await _bands.GetBandsAsync(user.Id);
        return includeClosed ? bands : bands.Where(b => !b.IsClosed).ToList();
    }

    public async Task<Band?> GetBandAsync(int bandId)
    {
        var band = await _bands.GetByIdAsync(bandId);

        // Sécurité : une bande n'est visible que par son propriétaire.
        return band is not null && band.UserId == _authService.CurrentUser?.Id ? band : null;
    }

    public async Task<BandRecords> GetBandRecordsAsync(Band band)
    {
        // Une seule requête au serveur pour les trois types de saisies.
        var set = await _records.GetAllRecordsAsync(band.Id);
        return new BandRecords(band, set.Eggs, set.Feeds, set.Mortality);
    }

    public async Task<BandRecords?> GetBandRecordsAsync(int bandId)
    {
        var band = await GetBandAsync(bandId);
        return band is null ? null : await GetBandRecordsAsync(band);
    }
}
