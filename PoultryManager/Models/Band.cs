namespace PoultryManager.Models;

/// <summary>
/// Bande : lot de poules pondeuses démarré à une date donnée avec un effectif initial.
/// </summary>
public sealed class Band
{
    public int Id { get; set; }

    /// <summary>Propriétaire de la bande.</summary>
    public int UserId { get; set; }

    /// <summary>Nom ou identifiant, ex. « Bande 1 ».</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Type ou souche (facultatif), ex. « Souche légère ».</summary>
    public string Strain { get; set; } = string.Empty;

    /// <summary>Date de démarrage (sans heure).</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Nombre de poules à l'arrivée.</summary>
    public int InitialCount { get; set; }

    /// <summary>Âge des poules à l'arrivée, en semaines (facultatif).</summary>
    public int? AgeAtArrivalWeeks { get; set; }

    public string Notes { get; set; } = string.Empty;

    public bool IsClosed { get; set; }

    /// <summary>Date de clôture (bande terminée).</summary>
    public DateTime? ClosedDate { get; set; }

    /// <summary>Utilisé par les listes déroulantes (Picker).</summary>
    public override string ToString() => Name;
}
