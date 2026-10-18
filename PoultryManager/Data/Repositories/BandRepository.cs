using PoultryManager.Models;

namespace PoultryManager.Data.Repositories;

/// <summary>Implémentation SQLite de <see cref="IBandRepository"/>.</summary>
public sealed class BandRepository : IBandRepository
{
    private readonly AppDatabase _database;

    public BandRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<IReadOnlyList<Band>> GetBandsAsync(int userId)
    {
        var db = await _database.GetConnectionAsync();
        var bands = await db.Table<Band>().Where(b => b.UserId == userId).ToListAsync();
        return bands
            .OrderBy(b => b.IsClosed)
            .ThenBy(b => b.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public async Task<Band?> GetByIdAsync(int bandId)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Band>().Where(b => b.Id == bandId).FirstOrDefaultAsync();
    }

    public async Task<bool> NameExistsAsync(int userId, string name, int? exceptBandId = null)
    {
        var except = exceptBandId ?? 0;
        var db = await _database.GetConnectionAsync();
        var bands = await db.Table<Band>().Where(b => b.UserId == userId && b.Id != except).ToListAsync();
        return bands.Any(b => string.Equals(b.Name.Trim(), name.Trim(), StringComparison.CurrentCultureIgnoreCase));
    }

    public async Task<int> AddAsync(Band band)
    {
        Normalize(band);
        var db = await _database.GetConnectionAsync();
        await db.InsertAsync(band);
        return band.Id;
    }

    public async Task UpdateAsync(Band band)
    {
        Normalize(band);
        var db = await _database.GetConnectionAsync();
        await db.UpdateAsync(band);
    }

    public async Task DeleteAsync(int bandId)
    {
        var db = await _database.GetConnectionAsync();
        await db.RunInTransactionAsync(connection =>
        {
            connection.Execute("DELETE FROM EggRecords WHERE BandId = ?", bandId);
            connection.Execute("DELETE FROM FeedRecords WHERE BandId = ?", bandId);
            connection.Execute("DELETE FROM MortalityRecords WHERE BandId = ?", bandId);
            connection.Execute("DELETE FROM Bands WHERE Id = ?", bandId);
        });
    }

    private static void Normalize(Band band)
    {
        band.Name = band.Name.Trim();
        band.StartDate = band.StartDate.Date;
        band.ClosedDate = band.ClosedDate?.Date;
    }
}
