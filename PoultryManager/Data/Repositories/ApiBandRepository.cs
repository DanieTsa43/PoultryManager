using PoultryManager.Models;
using PoultryManager.Services.Api;

namespace PoultryManager.Data.Repositories;

/// <summary>Implémentation de <see cref="IBandRepository"/> qui passe par l'API (SQL Server côté serveur).</summary>
public sealed class ApiBandRepository : IBandRepository
{
    private readonly IApiClient _api;

    public ApiBandRepository(IApiClient api)
    {
        _api = api;
    }

    // Le paramètre userId est conservé pour ne pas changer l'interface : le serveur lit l'utilisateur dans le jeton.
    public async Task<IReadOnlyList<Band>> GetBandsAsync(int userId)
    {
        var bands = await _api.GetAsync<List<Band>>("api/bands") ?? [];
        return bands
            .OrderBy(b => b.IsClosed)
            .ThenBy(b => b.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public Task<Band?> GetByIdAsync(int bandId)
        => _api.GetAsync<Band>($"api/bands/{bandId}");

    public async Task<bool> NameExistsAsync(int userId, string name, int? exceptBandId = null)
        => await _api.GetAsync<bool>(
            $"api/bands/name-exists?name={Uri.EscapeDataString(name.Trim())}&exceptBandId={exceptBandId ?? 0}");

    public async Task<int> AddAsync(Band band)
    {
        Normalize(band);
        var saved = await _api.PostAsync<Band>("api/bands", band)
            ?? throw new InvalidOperationException("Réponse vide du serveur.");
        CopyServerValues(saved, band);
        return band.Id;
    }

    public async Task UpdateAsync(Band band)
    {
        Normalize(band);
        var saved = await _api.PutAsync<Band>($"api/bands/{band.Id}", band)
            ?? throw new InvalidOperationException("Réponse vide du serveur.");
        CopyServerValues(saved, band);
    }

    public Task DeleteAsync(int bandId)
        => _api.DeleteAsync($"api/bands/{bandId}");

    private static void Normalize(Band band)
    {
        band.Name = band.Name.Trim();
        band.StartDate = band.StartDate.Date;
        band.ClosedDate = band.ClosedDate?.Date;
    }

    /// <summary>Reprend les valeurs décidées par le serveur (identifiant, propriétaire, date de clôture…).</summary>
    private static void CopyServerValues(Band saved, Band target)
    {
        target.Id = saved.Id;
        target.UserId = saved.UserId;
        target.Name = saved.Name;
        target.Strain = saved.Strain;
        target.StartDate = saved.StartDate;
        target.IsClosed = saved.IsClosed;
        target.ClosedDate = saved.ClosedDate;
    }
}
