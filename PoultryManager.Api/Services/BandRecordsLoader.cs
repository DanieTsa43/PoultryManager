using Microsoft.EntityFrameworkCore;
using PoultryManager.Api.Contracts;
using PoultryManager.Api.Data;

namespace PoultryManager.Api.Services;

/// <summary>Chargement de toutes les saisies d'une bande (utilisé par l'API et par la sauvegarde).</summary>
public static class BandRecordsLoader
{
    public static async Task<BandRecordsResponse> LoadBandRecordsAsync(this AppDbContext db, int bandId)
    {
        var eggs = await db.EggRecords.AsNoTracking().Where(r => r.BandId == bandId).OrderBy(r => r.Date).ToListAsync();
        var feeds = await db.FeedRecords.AsNoTracking().Where(r => r.BandId == bandId).OrderBy(r => r.Date).ToListAsync();
        var mortality = await db.MortalityRecords.AsNoTracking().Where(r => r.BandId == bandId).OrderBy(r => r.Date).ToListAsync();

        return new BandRecordsResponse(
            eggs.Select(r => r.ToDto()).ToList(),
            feeds.Select(r => r.ToDto()).ToList(),
            mortality.Select(r => r.ToDto()).ToList());
    }
}
