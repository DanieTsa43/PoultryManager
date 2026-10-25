namespace PoultryManager.Api.Domain;

/// <summary>Utilisateur (éleveur). Même structure que le modèle de l'application mobile.</summary>
public sealed class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<Band> Bands { get; set; } = [];
}

/// <summary>Bande : lot de poules pondeuses démarré à une date donnée.</summary>
public sealed class Band
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Strain { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public int InitialCount { get; set; }
    public int? AgeAtArrivalWeeks { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
    public DateTime? ClosedDate { get; set; }

    public List<EggRecord> EggRecords { get; set; } = [];
    public List<FeedRecord> FeedRecords { get; set; } = [];
    public List<MortalityRecord> MortalityRecords { get; set; } = [];
}

/// <summary>Collecte d'œufs d'une journée (un enregistrement par jour et par bande).</summary>
public sealed class EggRecord
{
    public int Id { get; set; }
    public int BandId { get; set; }
    public Band? Band { get; set; }
    public DateTime Date { get; set; }
    public int EggsCollected { get; set; }
    public int? BrokenEggs { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

/// <summary>Aliment distribué une journée (un enregistrement par jour et par bande).</summary>
public sealed class FeedRecord
{
    public int Id { get; set; }
    public int BandId { get; set; }
    public Band? Band { get; set; }
    public DateTime Date { get; set; }
    public string FeedType { get; set; } = string.Empty;
    public double QuantityKg { get; set; }
    public double? RemainingKg { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>Mortalité d'une journée (un enregistrement par jour et par bande).</summary>
public sealed class MortalityRecord
{
    public int Id { get; set; }
    public int BandId { get; set; }
    public Band? Band { get; set; }
    public DateTime Date { get; set; }
    public int Deaths { get; set; }
    public string Cause { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
