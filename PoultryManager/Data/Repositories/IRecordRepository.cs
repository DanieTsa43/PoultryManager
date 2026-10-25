using PoultryManager.Models;

namespace PoultryManager.Data.Repositories;

/// <summary>
/// Saisies quotidiennes : œufs, aliment, mortalité.
/// Une seule saisie de chaque type par jour et par bande : enregistrer à nouveau la même date
/// met à jour la saisie existante. Les données sont stockées sur le serveur (API ASP.NET Core + SQL Server).
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

    /// <summary>Toutes les saisies d'une bande (œufs, aliment, mortalité) en une seule requête.</summary>
    Task<BandRecordSet> GetAllRecordsAsync(int bandId);
}
