namespace PoultryManager.Models;

/// <summary>
/// Indicateurs calculés pour une journée d'une bande (non stocké : recalculé à partir des saisies).
/// Règles : voir docs/maquette/DESIGN.md, section 7.
/// </summary>
public sealed class DailyStat
{
    public DateTime Date { get; init; }

    /// <summary>Effectif en début de journée.</summary>
    public int StartCount { get; init; }

    /// <summary>Morts de la journée.</summary>
    public int Deaths { get; init; }

    /// <summary>Effectif en fin de journée.</summary>
    public int EndCount => StartCount - Deaths;

    /// <summary>Effectif vivant moyen = (début + fin) ÷ 2.</summary>
    public double AverageCount => (StartCount + EndCount) / 2.0;

    /// <summary>Œufs collectés (null si aucune saisie).</summary>
    public int? Eggs { get; init; }

    public int? BrokenEggs { get; init; }

    /// <summary>Aliment distribué en kg (null si aucune saisie).</summary>
    public double? FeedKg { get; init; }

    /// <summary>Vrai si au moins une saisie existe ce jour-là.</summary>
    public bool HasMortalityRecord { get; init; }

    public bool HasAnyRecord => Eggs.HasValue || FeedKg.HasValue || HasMortalityRecord;

    /// <summary>Taux de ponte (%) = œufs ÷ effectif moyen × 100. Null si non calculable.</summary>
    public double? LayingRate => Eggs.HasValue && AverageCount > 0 ? Eggs.Value / AverageCount * 100 : null;

    /// <summary>Consommation par poule (g) = kg × 1 000 ÷ effectif moyen. Null si non calculable.</summary>
    public double? FeedPerHenGrams => FeedKg.HasValue && AverageCount > 0 ? FeedKg.Value * 1000 / AverageCount : null;
}
