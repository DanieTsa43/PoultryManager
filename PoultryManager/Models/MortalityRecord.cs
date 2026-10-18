using SQLite;

namespace PoultryManager.Models;

/// <summary>Mortalité constatée une journée dans une bande (un enregistrement par jour et par bande).</summary>
[Table("MortalityRecords")]
public sealed class MortalityRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int BandId { get; set; }

    [Indexed]
    public DateTime Date { get; set; }

    /// <summary>Nombre de poules mortes.</summary>
    public int Deaths { get; set; }

    /// <summary>Cause présumée (facultative).</summary>
    public string Cause { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}
