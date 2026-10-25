namespace PoultryManager.Models;

/// <summary>Collecte d'œufs d'une journée pour une bande (un enregistrement par jour et par bande).</summary>
public sealed class EggRecord
{
    public int Id { get; set; }
    public int BandId { get; set; }

    /// <summary>Jour de la collecte (sans heure).</summary>
    public DateTime Date { get; set; }

    /// <summary>Nombre d'œufs collectés.</summary>
    public int EggsCollected { get; set; }

    /// <summary>Œufs cassés ou impropres (facultatif).</summary>
    public int? BrokenEggs { get; set; }

    /// <summary>Catégorie (Petit, Moyen, Gros…), facultative.</summary>
    public string Category { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}
