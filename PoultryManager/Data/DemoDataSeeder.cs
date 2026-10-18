using PoultryManager.Models;

namespace PoultryManager.Data;

/// <summary>
/// Génère le compte et les données de démonstration (fictives) de la maquette :
/// Jean Dupont · Ferme des Ailes, 3 bandes actives et 1 bande clôturée.
/// Les dates sont calculées par rapport à aujourd'hui pour que le tableau de bord soit toujours renseigné.
/// </summary>
public static class DemoDataSeeder
{
    public const string DemoEmail = "jean.dupont@exemple.com";
    public const string DemoUserName = "jean.dupont";
    public const string DemoPassword = "Poultry2025";

    /// <summary>Compte de démonstration (sans empreinte de mot de passe : calculée par l'appelant).</summary>
    public static User CreateDemoUser(DateTime now) => new()
    {
        FullName = "Jean Dupont",
        FarmName = "Ferme des Ailes",
        Email = DemoEmail,
        UserName = DemoUserName,
        Phone = string.Empty,
        CreatedAt = now
    };

    /// <summary>Définition d'une bande de démonstration.</summary>
    public sealed record DemoBand(string Name, string Strain, int DaysAgo, int InitialCount, int AgeWeeks, int? ClosedDaysAgo, int Seed);

    public static readonly IReadOnlyList<DemoBand> Bands =
    [
        new("Bande 1", "Souche légère", 47, 1800, 18, null, 11),
        new("Bande 2", "Souche mi-lourde", 95, 1500, 18, null, 23),
        new("Bande 3", "Souche légère", 130, 1700, 17, null, 37),
        new("Bande 4", "Souche mi-lourde", 560, 1500, 18, 140, 41)
    ];

    public static Band CreateBand(DemoBand demo, int userId, DateTime today) => new()
    {
        UserId = userId,
        Name = demo.Name,
        Strain = demo.Strain,
        StartDate = today.Date.AddDays(-demo.DaysAgo),
        InitialCount = demo.InitialCount,
        AgeAtArrivalWeeks = demo.AgeWeeks,
        Notes = "Données de démonstration.",
        IsClosed = demo.ClosedDaysAgo.HasValue,
        ClosedDate = demo.ClosedDaysAgo.HasValue ? today.Date.AddDays(-demo.ClosedDaysAgo.Value) : null
    };

    /// <summary>Saisies quotidiennes d'une bande, du démarrage à la clôture (ou à aujourd'hui).</summary>
    public static (List<EggRecord> Eggs, List<FeedRecord> Feeds, List<MortalityRecord> Mortality) CreateRecords(
        Band band, int seed, DateTime today)
    {
        var random = new Random(seed);
        var eggs = new List<EggRecord>();
        var feeds = new List<FeedRecord>();
        var mortality = new List<MortalityRecord>();

        var end = band.IsClosed && band.ClosedDate.HasValue ? band.ClosedDate.Value.Date : today.Date;
        var count = band.InitialCount;

        for (var (day, index) = (band.StartDate.Date, 0); day <= end; day = day.AddDays(1), index++)
        {
            // Mortalité : 1 mort par jour en moyenne au démarrage, moins ensuite.
            var roll = random.NextDouble();
            var deaths = index < 70
                ? (roll < 0.22 ? 0 : roll < 0.78 ? 1 : roll < 0.97 ? 2 : 3)
                : (roll < 0.62 ? 0 : roll < 0.95 ? 1 : 2);
            deaths = Math.Min(deaths, count);
            var average = count - deaths / 2.0;

            if (deaths > 0)
            {
                mortality.Add(new MortalityRecord
                {
                    BandId = band.Id,
                    Date = day,
                    Deaths = deaths,
                    Cause = random.NextDouble() < 0.7 ? "Inconnue" : random.NextDouble() < 0.6 ? "Maladie" : "Chaleur",
                    Notes = string.Empty
                });
            }

            // Ponte : montée en ponte sur 7 semaines puis plateau et lente décroissance.
            var rate = Math.Clamp(LayingCurve(index) + (random.NextDouble() - 0.5) * 1.6, 0, 96);
            var collected = (int)Math.Round(average * rate / 100);
            eggs.Add(new EggRecord
            {
                BandId = band.Id,
                Date = day,
                EggsCollected = collected,
                BrokenEggs = (int)Math.Round(collected * (0.008 + random.NextDouble() * 0.006)),
                Category = "Moyen",
                Notes = string.Empty
            });

            // Aliment : de 95 g à 118 g par poule et par jour.
            var gramsPerHen = Math.Min(95 + index * 0.48, 118) + (random.NextDouble() - 0.5) * 2;
            feeds.Add(new FeedRecord
            {
                BandId = band.Id,
                Date = day,
                FeedType = index < 14 ? "Aliment pré-ponte" : "Aliment pondeuse",
                QuantityKg = Math.Round(average * gramsPerHen / 1000),
                RemainingKg = null,
                Notes = string.Empty
            });

            count -= deaths;
        }

        return (eggs, feeds, mortality);
    }

    /// <summary>Taux de ponte cible (%) selon le jour depuis le démarrage.</summary>
    private static double LayingCurve(int dayIndex)
    {
        double[] firstWeeks = [20, 41, 59, 70, 78, 84, 85.8];
        var week = dayIndex / 7;
        if (week < firstWeeks.Length)
        {
            return firstWeeks[week];
        }

        if (week < 30)
        {
            return Math.Min(88, 85.8 + (week - 6) * 0.5);
        }

        return Math.Max(66, 88 - (week - 30) * 0.4);
    }
}
