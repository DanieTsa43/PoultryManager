namespace PoultryManager.Models;

/// <summary>Mortalité constatée une journée dans une bande (un enregistrement par jour et par bande).</summary>
public sealed class MortalityRecord
{
    public int Id { get; set; }
    public int BandId { get; set; }
    public DateTime Date { get; set; }

    /// <summary>Nombre de poules mortes.</summary>
    public int Deaths { get; set; }

    /// <summary>Cause présumée (facultative).</summary>
    public string Cause { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}
