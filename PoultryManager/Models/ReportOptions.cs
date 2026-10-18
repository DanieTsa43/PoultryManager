namespace PoultryManager.Models;

/// <summary>Contenu choisi sur l'écran 11 · Rapports et exportation.</summary>
public sealed record ReportOptions(
    bool IncludeCount,
    bool IncludeEggs,
    bool IncludeFeed,
    bool IncludeMortality,
    bool IncludeLayingRate);

/// <summary>Rapport prêt à être exporté (PDF ou CSV).</summary>
public sealed class ReportData
{
    public required Band Band { get; init; }

    public required string FarmName { get; init; }

    public required string AuthorName { get; init; }

    public DateTime From { get; init; }

    public DateTime To { get; init; }

    public required ReportOptions Options { get; init; }

    /// <summary>Lignes jour par jour.</summary>
    public required IReadOnlyList<DailyStat> Days { get; init; }

    /// <summary>Bilan de la bande (du démarrage à la fin de la période du rapport).</summary>
    public required BandSummary Summary { get; init; }

    public DateTime GeneratedAt { get; init; }

    public int DayCount => Days.Count;

    public int TotalEggs => Days.Sum(d => d.Eggs ?? 0);

    public double TotalFeedKg => Days.Sum(d => d.FeedKg ?? 0);

    public int TotalDeaths => Days.Sum(d => d.Deaths);

    /// <summary>Taux de ponte moyen sur la période du rapport.</summary>
    public double? AverageLayingRate
    {
        get
        {
            var henDays = Days.Where(d => d.Eggs.HasValue).Sum(d => d.AverageCount);
            return henDays > 0 ? TotalEggs / henDays * 100 : null;
        }
    }
}
