using PoultryManager.Models;

namespace PoultryManager.Data.Repositories;

/// <summary>
/// Saisies quotidiennes : œufs, aliment, mortalité.
/// Une seule saisie de chaque type par jour et par bande : enregistrer à nouveau la même date
/// met à jour la saisie existante.
/// </summary>
public interface IRecordRepository
{
    // ----- Œufs -----
    Task<IReadOnlyList<EggRecord>> GetEggRecordsAsync(int bandId, DateTime? from = null, DateTime? to = null);

    Task<EggRecord?> GetEggRecordAsync(int bandId, DateTime date);

    Task SaveEggRecordAsync(EggRecord record);

    Task DeleteEggRecordAsync(int recordId);

    // ----- Aliment -----
    Task<IReadOnlyList<FeedRecord>> GetFeedRecordsAsync(int bandId, DateTime? from = null, DateTime? to = null);

    Task<FeedRecord?> GetFeedRecordAsync(int bandId, DateTime date);

    Task SaveFeedRecordAsync(FeedRecord record);

    Task DeleteFeedRecordAsync(int recordId);

    // ----- Mortalité -----
    Task<IReadOnlyList<MortalityRecord>> GetMortalityRecordsAsync(int bandId, DateTime? from = null, DateTime? to = null);

    Task<MortalityRecord?> GetMortalityRecordAsync(int bandId, DateTime date);

    Task SaveMortalityRecordAsync(MortalityRecord record);

    Task DeleteMortalityRecordAsync(int recordId);

    // ----- Divers -----

    /// <summary>Supprime toutes les saisies d'une bande pour un jour donné.</summary>
    Task DeleteDayAsync(int bandId, DateTime date);

    /// <summary>Insertion groupée (données de démonstration).</summary>
    Task AddRangeAsync(IEnumerable<EggRecord> eggs, IEnumerable<FeedRecord> feeds, IEnumerable<MortalityRecord> mortality);
}
