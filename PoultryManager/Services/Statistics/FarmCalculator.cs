using PoultryManager.Models;

namespace PoultryManager.Services.Statistics;

/// <summary>
/// Règles de calcul de l'élevage (DESIGN.md, section 7). Classe pure, sans accès aux données :
/// elle reçoit les saisies et renvoie les indicateurs.
/// <list type="bullet">
/// <item>Effectif actuel = effectif initial − mortalité cumulée.</item>
/// <item>Effectif vivant moyen du jour = (début de journée + fin de journée) ÷ 2.</item>
/// <item>Taux de ponte (%) = œufs collectés ÷ effectif vivant moyen × 100.</item>
/// <item>Consommation par poule (g) = kg distribués × 1 000 ÷ effectif vivant moyen.</item>
/// <item>Taux de mortalité cumulé (%) = mortalité cumulée ÷ effectif initial × 100.</item>
/// <item>Indice de conversion = kg d'aliment ÷ kg d'œufs (œufs × poids moyen d'un œuf).</item>
/// </list>
/// </summary>
public static class FarmCalculator
{
    /// <summary>Effectif à la fin de la journée <paramref name="asOf"/> (ou effectif actuel si null).</summary>
    public static int CurrentCount(Band band, IEnumerable<MortalityRecord> mortality, DateTime? asOf = null)
    {
        var limit = asOf?.Date;
        var deaths = mortality
            .Where(m => m.BandId == band.Id && (limit is null || m.Date.Date <= limit))
            .Sum(m => m.Deaths);
        return band.InitialCount - deaths;
    }

    /// <summary>Effectif au début de la journée <paramref name="date"/> (morts du jour non déduits).</summary>
    public static int CountAtStartOfDay(Band band, IEnumerable<MortalityRecord> mortality, DateTime date)
    {
        var day = date.Date;
        var deaths = mortality
            .Where(m => m.BandId == band.Id && m.Date.Date < day)
            .Sum(m => m.Deaths);
        return band.InitialCount - deaths;
    }

    /// <summary>Mortalité cumulée jusqu'au jour <paramref name="asOf"/> inclus.</summary>
    public static int CumulativeDeaths(Band band, IEnumerable<MortalityRecord> mortality, DateTime? asOf = null)
        => band.InitialCount - CurrentCount(band, mortality, asOf);

    /// <summary>
    /// Indicateurs jour par jour de <paramref name="from"/> à <paramref name="to"/> (bornes incluses),
    /// sans remonter avant le démarrage de la bande.
    /// </summary>
    public static IReadOnlyList<DailyStat> BuildDailyStats(
        Band band,
        IEnumerable<EggRecord> eggRecords,
        IEnumerable<FeedRecord> feedRecords,
        IEnumerable<MortalityRecord> mortalityRecords,
        DateTime from,
        DateTime to)
    {
        var start = from.Date < band.StartDate.Date ? band.StartDate.Date : from.Date;
        var end = to.Date;
        if (end < start)
        {
            return [];
        }

        var deathsByDay = mortalityRecords
            .Where(m => m.BandId == band.Id)
            .GroupBy(m => m.Date.Date)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Deaths));

        var eggsByDay = eggRecords
            .Where(e => e.BandId == band.Id)
            .GroupBy(e => e.Date.Date)
            .ToDictionary(
                g => g.Key,
                g => (Eggs: g.Sum(e => e.EggsCollected),
                      Broken: g.Any(e => e.BrokenEggs.HasValue) ? g.Sum(e => e.BrokenEggs ?? 0) : (int?)null));

        var feedByDay = feedRecords
            .Where(f => f.BandId == band.Id)
            .GroupBy(f => f.Date.Date)
            .ToDictionary(g => g.Key, g => g.Sum(f => f.QuantityKg));

        var count = band.InitialCount - deathsByDay.Where(kv => kv.Key < start).Sum(kv => kv.Value);

        var result = new List<DailyStat>((end - start).Days + 1);
        for (var day = start; day <= end; day = day.AddDays(1))
        {
            var hasDeaths = deathsByDay.TryGetValue(day, out var deaths);
            var hasEggs = eggsByDay.TryGetValue(day, out var eggs);
            var hasFeed = feedByDay.TryGetValue(day, out var feedKg);

            result.Add(new DailyStat
            {
                Date = day,
                StartCount = count,
                Deaths = hasDeaths ? deaths : 0,
                HasMortalityRecord = hasDeaths,
                Eggs = hasEggs ? eggs.Eggs : null,
                BrokenEggs = hasEggs ? eggs.Broken : null,
                FeedKg = hasFeed ? feedKg : null
            });

            count -= hasDeaths ? deaths : 0;
        }

        return result;
    }

    /// <summary>Indice de conversion alimentaire. Null si l'une des deux données manque.</summary>
    public static double? FeedConversionRatio(double? feedKg, int? eggs, double eggWeightGrams)
    {
        if (feedKg is not > 0 || eggs is not > 0 || eggWeightGrams <= 0)
        {
            return null;
        }

        var eggMassKg = eggs.Value * eggWeightGrams / 1000;
        return feedKg.Value / eggMassKg;
    }

    /// <summary>Fin de la période d'une bande : date de clôture, sinon aujourd'hui.</summary>
    public static DateTime PeriodEnd(Band band, DateTime today)
    {
        var end = band.IsClosed && band.ClosedDate.HasValue ? band.ClosedDate.Value.Date : today.Date;
        return end < band.StartDate.Date ? band.StartDate.Date : end;
    }

    /// <summary>Bilan complet d'une bande, du démarrage à la clôture (ou à aujourd'hui).</summary>
    public static BandSummary BuildSummary(
        Band band,
        IEnumerable<EggRecord> eggRecords,
        IEnumerable<FeedRecord> feedRecords,
        IEnumerable<MortalityRecord> mortalityRecords,
        DateTime today)
    {
        var from = band.StartDate.Date;
        var to = PeriodEnd(band, today);
        var mortality = mortalityRecords.ToList();
        var stats = BuildDailyStats(band, eggRecords, feedRecords, mortality, from, to);

        var cumulativeDeaths = CumulativeDeaths(band, mortality, to);

        var weekly = stats
            .Select((stat, index) => (stat, week: index / 7))
            .GroupBy(x => x.week)
            .Select(g =>
            {
                var days = g.Select(x => x.stat).ToList();
                var layingDays = days.Where(d => d.Eggs.HasValue).ToList();
                var henDays = layingDays.Sum(d => d.AverageCount);
                double? rate = henDays > 0 ? layingDays.Sum(d => d.Eggs!.Value) / henDays * 100 : null;
                return new WeeklyRate(g.Key + 1, days[0].Date, days[^1].Date, rate);
            })
            .ToList();

        return new BandSummary
        {
            Band = band,
            From = from,
            To = to,
            Days = stats.Count,
            InitialCount = band.InitialCount,
            FinalCount = band.InitialCount - cumulativeDeaths,
            TotalEggs = stats.Sum(s => s.Eggs ?? 0),
            TotalFeedKg = stats.Sum(s => s.FeedKg ?? 0),
            CumulativeDeaths = cumulativeDeaths,
            HenDays = stats.Where(s => s.Eggs.HasValue).Sum(s => s.AverageCount),
            WeeklyRates = weekly,
            DailyStats = stats
        };
    }
}
