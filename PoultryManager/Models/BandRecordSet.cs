namespace PoultryManager.Models;

/// <summary>Toutes les saisies d'une bande (œufs, aliment, mortalité), obtenues en une seule requête.</summary>
public sealed record BandRecordSet(
    IReadOnlyList<EggRecord> Eggs,
    IReadOnlyList<FeedRecord> Feeds,
    IReadOnlyList<MortalityRecord> Mortality)
{
    public static BandRecordSet Empty { get; } = new([], [], []);
}
