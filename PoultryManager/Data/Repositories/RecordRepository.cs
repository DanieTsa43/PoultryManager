using PoultryManager.Models;

namespace PoultryManager.Data.Repositories;

/// <summary>Implémentation SQLite de <see cref="IRecordRepository"/>.</summary>
public sealed class RecordRepository : IRecordRepository
{
    private readonly AppDatabase _database;

    public RecordRepository(AppDatabase database)
    {
        _database = database;
    }

    // Les dates sont stockées sans heure ; une période [from ; to] inclut toute la journée « to ».
    private static DateTime Lower(DateTime? from) => from?.Date ?? DateTime.MinValue;

    private static DateTime Upper(DateTime? to) => to?.Date ?? DateTime.MaxValue.Date;

    // =============== Œufs ===============

    public async Task<IReadOnlyList<EggRecord>> GetEggRecordsAsync(int bandId, DateTime? from = null, DateTime? to = null)
    {
        var (lower, upper) = (Lower(from), Upper(to));
        var db = await _database.GetConnectionAsync();
        return await db.Table<EggRecord>()
            .Where(r => r.BandId == bandId && r.Date >= lower && r.Date <= upper)
            .OrderBy(r => r.Date)
            .ToListAsync();
    }

    public async Task<EggRecord?> GetEggRecordAsync(int bandId, DateTime date)
    {
        var day = date.Date;
        var db = await _database.GetConnectionAsync();
        return await db.Table<EggRecord>().Where(r => r.BandId == bandId && r.Date == day).FirstOrDefaultAsync();
    }

    public async Task SaveEggRecordAsync(EggRecord record)
    {
        record.Date = record.Date.Date;
        var existing = await GetEggRecordAsync(record.BandId, record.Date);
        var db = await _database.GetConnectionAsync();

        if (record.Id == 0 && existing is not null)
        {
            record.Id = existing.Id;
        }
        else if (record.Id != 0 && existing is not null && existing.Id != record.Id)
        {
            // La date a été changée vers un jour déjà saisi : on remplace la saisie de ce jour.
            await db.DeleteAsync<EggRecord>(existing.Id);
        }

        if (record.Id == 0)
        {
            await db.InsertAsync(record);
        }
        else
        {
            await db.UpdateAsync(record);
        }
    }

    public async Task DeleteEggRecordAsync(int recordId)
    {
        var db = await _database.GetConnectionAsync();
        await db.DeleteAsync<EggRecord>(recordId);
    }

    // =============== Aliment ===============

    public async Task<IReadOnlyList<FeedRecord>> GetFeedRecordsAsync(int bandId, DateTime? from = null, DateTime? to = null)
    {
        var (lower, upper) = (Lower(from), Upper(to));
        var db = await _database.GetConnectionAsync();
        return await db.Table<FeedRecord>()
            .Where(r => r.BandId == bandId && r.Date >= lower && r.Date <= upper)
            .OrderBy(r => r.Date)
            .ToListAsync();
    }

    public async Task<FeedRecord?> GetFeedRecordAsync(int bandId, DateTime date)
    {
        var day = date.Date;
        var db = await _database.GetConnectionAsync();
        return await db.Table<FeedRecord>().Where(r => r.BandId == bandId && r.Date == day).FirstOrDefaultAsync();
    }

    public async Task SaveFeedRecordAsync(FeedRecord record)
    {
        record.Date = record.Date.Date;
        var existing = await GetFeedRecordAsync(record.BandId, record.Date);
        var db = await _database.GetConnectionAsync();

        if (record.Id == 0 && existing is not null)
        {
            record.Id = existing.Id;
        }
        else if (record.Id != 0 && existing is not null && existing.Id != record.Id)
        {
            await db.DeleteAsync<FeedRecord>(existing.Id);
        }

        if (record.Id == 0)
        {
            await db.InsertAsync(record);
        }
        else
        {
            await db.UpdateAsync(record);
        }
    }

    public async Task DeleteFeedRecordAsync(int recordId)
    {
        var db = await _database.GetConnectionAsync();
        await db.DeleteAsync<FeedRecord>(recordId);
    }

    // =============== Mortalité ===============

    public async Task<IReadOnlyList<MortalityRecord>> GetMortalityRecordsAsync(int bandId, DateTime? from = null, DateTime? to = null)
    {
        var (lower, upper) = (Lower(from), Upper(to));
        var db = await _database.GetConnectionAsync();
        return await db.Table<MortalityRecord>()
            .Where(r => r.BandId == bandId && r.Date >= lower && r.Date <= upper)
            .OrderBy(r => r.Date)
            .ToListAsync();
    }

    public async Task<MortalityRecord?> GetMortalityRecordAsync(int bandId, DateTime date)
    {
        var day = date.Date;
        var db = await _database.GetConnectionAsync();
        return await db.Table<MortalityRecord>().Where(r => r.BandId == bandId && r.Date == day).FirstOrDefaultAsync();
    }

    public async Task SaveMortalityRecordAsync(MortalityRecord record)
    {
        record.Date = record.Date.Date;
        var existing = await GetMortalityRecordAsync(record.BandId, record.Date);
        var db = await _database.GetConnectionAsync();

        if (record.Id == 0 && existing is not null)
        {
            record.Id = existing.Id;
        }
        else if (record.Id != 0 && existing is not null && existing.Id != record.Id)
        {
            await db.DeleteAsync<MortalityRecord>(existing.Id);
        }

        if (record.Id == 0)
        {
            await db.InsertAsync(record);
        }
        else
        {
            await db.UpdateAsync(record);
        }
    }

    public async Task DeleteMortalityRecordAsync(int recordId)
    {
        var db = await _database.GetConnectionAsync();
        await db.DeleteAsync<MortalityRecord>(recordId);
    }

    // =============== Divers ===============

    public async Task DeleteDayAsync(int bandId, DateTime date)
    {
        var ticks = date.Date.Ticks;
        var db = await _database.GetConnectionAsync();
        await db.RunInTransactionAsync(connection =>
        {
            connection.Execute("DELETE FROM EggRecords WHERE BandId = ? AND Date = ?", bandId, ticks);
            connection.Execute("DELETE FROM FeedRecords WHERE BandId = ? AND Date = ?", bandId, ticks);
            connection.Execute("DELETE FROM MortalityRecords WHERE BandId = ? AND Date = ?", bandId, ticks);
        });
    }

    public async Task AddRangeAsync(IEnumerable<EggRecord> eggs, IEnumerable<FeedRecord> feeds, IEnumerable<MortalityRecord> mortality)
    {
        var db = await _database.GetConnectionAsync();
        await db.InsertAllAsync(eggs.ToList());
        await db.InsertAllAsync(feeds.ToList());
        await db.InsertAllAsync(mortality.ToList());
    }
}
