using Microsoft.EntityFrameworkCore;
using PoultryManager.Api.Domain;
using PoultryManager.Api.Services;

namespace PoultryManager.Api.Data;

/// <summary>
/// Compte et données de démonstration (fictives) : Jean Dupont · Ferme des Ailes,
/// 3 bandes actives et 1 bande clôturée. Créés au premier démarrage de l'API si la base est vide
/// et si « Seed:DemoData » vaut true (activé dans appsettings.Development.json).
/// Les dates sont calculées par rapport à aujourd'hui pour que le tableau de bord soit toujours renseigné.
/// </summary>
public static class DemoDataSeeder
{
    public const string DemoEmail = "jean.dupont@exemple.com";
    public const string DemoUserName = "jean.dupont";
    public const string DemoPassword = "Poultry2025";

    private sealed record DemoBand(string Name, string Strain, int DaysAgo, int InitialCount, int AgeWeeks, int? ClosedDaysAgo, int Seed);

    private static readonly IReadOnlyList<DemoBand> Bands =
    [
        new("Bande 1", "Souche légère", 47, 1800, 18, null, 11),
        new("Bande 2", "Souche mi-lourde", 95, 1500, 18, null, 23),
        new("Bande 3", "Souche légère", 130, 1700, 17, null, 37),
        new("Bande 4", "Souche mi-lourde", 560, 1500, 18, 140, 41)
    ];

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var today = FarmRules.Today;
        var (hash, salt) = PasswordHasher.Hash(DemoPassword);
        var user = new User
        {
            FullName = "Jean Dupont",
            FarmName = "Ferme des Ailes",
            Email = DemoEmail,
            UserName = DemoUserName,
            Phone = string.Empty,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var demo in Bands)
        {
            var band = new Band
            {
                Name = demo.Name,
                Strain = demo.Strain,
                StartDate = today.AddDays(-demo.DaysAgo),
                InitialCount = demo.InitialCount,
                AgeAtArrivalWeeks = demo.AgeWeeks,
                Notes = "Données de démonstration.",
                IsClosed = demo.ClosedDaysAgo.HasValue,
                ClosedDate = demo.ClosedDaysAgo.HasValue ? today.AddDays(-demo.ClosedDaysAgo.Value) : null
            };

            FillRecords(band, demo.Seed, today);
            user.Bands.Add(band);
        }

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    /// <summary>Saisies quotidiennes d'une bande, du démarrage à la clôture (ou à aujourd'hui).</summary>
    private static void FillRecords(Band band, int seed, DateTime today)
    {
        var random = new Random(seed);
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
                band.MortalityRecords.Add(new MortalityRecord
                {
                    Date = day,
                    Deaths = deaths,
                    Cause = random.NextDouble() < 0.7 ? "Inconnue" : random.NextDouble() < 0.6 ? "Maladie" : "Chaleur",
                    Notes = string.Empty
                });
            }

            // Ponte : montée en ponte sur 7 semaines puis plateau et lente décroissance.
            var rate = Math.Clamp(LayingCurve(index) + (random.NextDouble() - 0.5) * 1.6, 0, 96);
            var collected = (int)Math.Round(average * rate / 100);
            band.EggRecords.Add(new EggRecord
            {
                Date = day,
                EggsCollected = collected,
                BrokenEggs = (int)Math.Round(collected * (0.008 + random.NextDouble() * 0.006)),
                Category = "Moyen",
                Notes = string.Empty
            });

            // Aliment : de 95 g à 118 g par poule et par jour.
            var gramsPerHen = Math.Min(95 + index * 0.48, 118) + (random.NextDouble() - 0.5) * 2;
            band.FeedRecords.Add(new FeedRecord
            {
                Date = day,
                FeedType = index < 14 ? "Aliment pré-ponte" : "Aliment pondeuse",
                QuantityKg = Math.Round(average * gramsPerHen / 1000),
                RemainingKg = null,
                Notes = string.Empty
            });

            count -= deaths;
        }
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
