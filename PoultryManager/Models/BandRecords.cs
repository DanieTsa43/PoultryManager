using PoultryManager.Services.Statistics;

namespace PoultryManager.Models;

/// <summary>
/// Une bande et toutes ses saisies, avec les calculs usuels (délègue à <see cref="FarmCalculator"/>).
/// </summary>
public sealed class BandRecords
{
    public BandRecords(Band band, IReadOnlyList<EggRecord> eggs, IReadOnlyList<FeedRecord> feeds, IReadOnlyList<MortalityRecord> mortality)
    {
        Band = band;
        Eggs = eggs;
        Feeds = feeds;
        Mortality = mortality;
    }

    public Band Band { get; }

    public IReadOnlyList<EggRecord> Eggs { get; }

    public IReadOnlyList<FeedRecord> Feeds { get; }

    public IReadOnlyList<MortalityRecord> Mortality { get; }

    /// <summary>Effectif actuel (ou à la fin du jour indiqué).</summary>
    public int CurrentCount(DateTime? asOf = null) => FarmCalculator.CurrentCount(Band, Mortality, asOf);

    /// <summary>Effectif en début de journée.</summary>
    public int CountAtStartOfDay(DateTime date) => FarmCalculator.CountAtStartOfDay(Band, Mortality, date);

    /// <summary>Morts enregistrés un jour donné (hors saisie à exclure, pour une modification).</summary>
    public int DeathsOn(DateTime date, int? excludeRecordId = null)
        => Mortality.Where(m => m.Date.Date == date.Date && m.Id != excludeRecordId).Sum(m => m.Deaths);

    /// <summary>Indicateurs jour par jour sur la période.</summary>
    public IReadOnlyList<DailyStat> DailyStats(DateTime from, DateTime to)
        => FarmCalculator.BuildDailyStats(Band, Eggs, Feeds, Mortality, from, to);

    /// <summary>Indicateurs d'une seule journée.</summary>
    public DailyStat? DayStat(DateTime date) => DailyStats(date, date).FirstOrDefault();

    /// <summary>Bilan complet de la bande.</summary>
    public BandSummary Summary(DateTime today) => FarmCalculator.BuildSummary(Band, Eggs, Feeds, Mortality, today);

    /// <summary>Fin de période de la bande : date de clôture, sinon aujourd'hui.</summary>
    public DateTime PeriodEnd(DateTime today) => FarmCalculator.PeriodEnd(Band, today);

    /// <summary>Premier jour ayant une saisie (null si aucune).</summary>
    public DateTime? FirstRecordDate
    {
        get
        {
            var dates = Eggs.Select(e => e.Date).Concat(Feeds.Select(f => f.Date)).Concat(Mortality.Select(m => m.Date)).ToList();
            return dates.Count == 0 ? null : dates.Min().Date;
        }
    }
}
