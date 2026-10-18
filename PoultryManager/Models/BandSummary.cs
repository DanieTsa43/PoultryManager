namespace PoultryManager.Models;

/// <summary>Bilan d'une bande sur une période (écran 10 · Bilan par bande).</summary>
public sealed class BandSummary
{
    public required Band Band { get; init; }

    public DateTime From { get; init; }

    public DateTime To { get; init; }

    /// <summary>Nombre de jours de la période (bornes incluses).</summary>
    public int Days { get; init; }

    public int InitialCount { get; init; }

    /// <summary>Effectif à la fin de la période.</summary>
    public int FinalCount { get; init; }

    public int TotalEggs { get; init; }

    public double TotalFeedKg { get; init; }

    /// <summary>Mortalité cumulée depuis le démarrage de la bande.</summary>
    public int CumulativeDeaths { get; init; }

    /// <summary>Taux de mortalité cumulé (%) = mortalité cumulée ÷ effectif initial × 100.</summary>
    public double? MortalityRate => InitialCount > 0 ? CumulativeDeaths * 100.0 / InitialCount : null;

    /// <summary>Somme des effectifs moyens journaliers sur les jours où des œufs ont été saisis.</summary>
    public double HenDays { get; init; }

    /// <summary>Taux moyen de ponte (%) = total d'œufs ÷ poules-jours × 100.</summary>
    public double? AverageLayingRate => HenDays > 0 ? TotalEggs / HenDays * 100 : null;

    /// <summary>Taux de ponte par semaine depuis le démarrage (S1, S2…).</summary>
    public IReadOnlyList<WeeklyRate> WeeklyRates { get; init; } = [];

    /// <summary>Détail jour par jour de la période.</summary>
    public IReadOnlyList<DailyStat> DailyStats { get; init; } = [];
}

/// <summary>Taux de ponte d'une semaine de la bande.</summary>
public sealed record WeeklyRate(int WeekNumber, DateTime From, DateTime To, double? LayingRate);
