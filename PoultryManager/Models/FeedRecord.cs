using SQLite;

namespace PoultryManager.Models;

/// <summary>Aliment distribué une journée à une bande (un enregistrement par jour et par bande).</summary>
[Table("FeedRecords")]
public sealed class FeedRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int BandId { get; set; }

    [Indexed]
    public DateTime Date { get; set; }

    /// <summary>Type d'aliment, ex. « Aliment pondeuse ».</summary>
    public string FeedType { get; set; } = string.Empty;

    /// <summary>Quantité distribuée, en kg.</summary>
    public double QuantityKg { get; set; }

    /// <summary>Quantité restante mesurée, en kg (facultative).</summary>
    public double? RemainingKg { get; set; }

    public string Notes { get; set; } = string.Empty;
}
