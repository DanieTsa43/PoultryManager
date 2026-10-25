using System.Globalization;
using PoultryManager.Models;
using PoultryManager.Services.Api;

namespace PoultryManager.Data.Repositories;

/// <summary>
/// Implémentation de <see cref="IRecordRepository"/> qui passe par l'API.
/// Le serveur applique les mêmes règles que les formulaires (une saisie par jour et par bande, contrôles d'effectif…).
/// </summary>
public sealed class ApiRecordRepository : IRecordRepository
{
    private readonly IApiClient _api;

    public ApiRecordRepository(IApiClient api)
    {
        _api = api;
    }

    private static string Day(DateTime date) => date.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string Period(DateTime? from, DateTime? to)
    {
        var parts = new List<string>();
        if (from.HasValue)
        {
            parts.Add($"from={Day(from.Value)}");
        }

        if (to.HasValue)
        {
            parts.Add($"to={Day(to.Value)}");
        }

        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    private static T Required<T>(T? value) where T : class
        => value ?? throw new InvalidOperationException("Réponse vide du serveur.");

    // =============== Œufs ===============

    public async Task<IReadOnlyList<EggRecord>> GetEggRecordsAsync(int bandId, DateTime? from = null, DateTime? to = null)
        => await _api.GetAsync<List<EggRecord>>($"api/bands/{bandId}/eggs{Period(from, to)}") ?? [];

    public Task<EggRecord?> GetEggRecordAsync(int bandId, DateTime date)
        => _api.GetAsync<EggRecord>($"api/bands/{bandId}/eggs/{Day(date)}");

    public async Task SaveEggRecordAsync(EggRecord record)
    {
        record.Date = record.Date.Date;
        var saved = Required(await _api.PutAsync<EggRecord>($"api/bands/{record.BandId}/eggs", record));
        record.Id = saved.Id;
    }

    public Task DeleteEggRecordAsync(int recordId)
        => _api.DeleteAsync($"api/eggs/{recordId}");

    // =============== Aliment ===============

    public async Task<IReadOnlyList<FeedRecord>> GetFeedRecordsAsync(int bandId, DateTime? from = null, DateTime? to = null)
        => await _api.GetAsync<List<FeedRecord>>($"api/bands/{bandId}/feeds{Period(from, to)}") ?? [];

    public Task<FeedRecord?> GetFeedRecordAsync(int bandId, DateTime date)
        => _api.GetAsync<FeedRecord>($"api/bands/{bandId}/feeds/{Day(date)}");

    public async Task SaveFeedRecordAsync(FeedRecord record)
    {
        record.Date = record.Date.Date;
        var saved = Required(await _api.PutAsync<FeedRecord>($"api/bands/{record.BandId}/feeds", record));
        record.Id = saved.Id;
    }

    public Task DeleteFeedRecordAsync(int recordId)
        => _api.DeleteAsync($"api/feeds/{recordId}");

    // =============== Mortalité ===============

    public async Task<IReadOnlyList<MortalityRecord>> GetMortalityRecordsAsync(int bandId, DateTime? from = null, DateTime? to = null)
        => await _api.GetAsync<List<MortalityRecord>>($"api/bands/{bandId}/mortality{Period(from, to)}") ?? [];

    public Task<MortalityRecord?> GetMortalityRecordAsync(int bandId, DateTime date)
        => _api.GetAsync<MortalityRecord>($"api/bands/{bandId}/mortality/{Day(date)}");

    public async Task SaveMortalityRecordAsync(MortalityRecord record)
    {
        record.Date = record.Date.Date;
        var saved = Required(await _api.PutAsync<MortalityRecord>($"api/bands/{record.BandId}/mortality", record));
        record.Id = saved.Id;
    }

    public Task DeleteMortalityRecordAsync(int recordId)
        => _api.DeleteAsync($"api/mortality/{recordId}");

    // =============== Divers ===============

    public Task DeleteDayAsync(int bandId, DateTime date)
        => _api.DeleteAsync($"api/bands/{bandId}/days/{Day(date)}");

    public async Task<BandRecordSet> GetAllRecordsAsync(int bandId)
    {
        var payload = await _api.GetAsync<BandRecordsPayload>($"api/bands/{bandId}/records");
        return payload is null
            ? BandRecordSet.Empty
            : new BandRecordSet(payload.Eggs, payload.Feeds, payload.Mortality);
    }

    private sealed class BandRecordsPayload
    {
        public List<EggRecord> Eggs { get; set; } = [];

        public List<FeedRecord> Feeds { get; set; } = [];

        public List<MortalityRecord> Mortality { get; set; } = [];
    }
}
