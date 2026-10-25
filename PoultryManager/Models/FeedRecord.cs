namespace PoultryManager.Models;

/// <summary>Aliment distribué une journée à une bande (un enregistrement par jour et par bande).</summary>
public sealed class FeedRecord
{
    public int Id { get; set; }
    public int BandId { get; set; }
    public DateTime Date { get; set; }

    /// <summary>Type d'aliment, ex. « Aliment pondeuse ».</summary>
    public string FeedType { get; set; } = string.Empty;

    /// <summary>Quantité distribuée, en kg.</summary>
    public double QuantityKg { get; set; }

    /// <summary>Quantité restante mesurée, en kg (facultative).</summary>
    public double? RemainingKg { get; set; }

    public string Notes { get; set; } = string.Empty;
}
