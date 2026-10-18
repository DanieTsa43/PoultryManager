namespace PoultryManager.ViewModels.Items;

/// <summary>Carte « libellé + valeur » (bilan, indicateurs).</summary>
/// <param name="Label">Libellé, ex. « Effectif initial ».</param>
/// <param name="Value">Valeur mise en forme, ex. « 1 800 ».</param>
/// <param name="Extra">Complément affiché en rouge à côté de la valeur (facultatif), ex. « 2,8 % ».</param>
/// <param name="IsHighlighted">Carte mise en avant (fond vert forêt).</param>
public sealed record SummaryTile(string Label, string Value, string? Extra = null, bool IsHighlighted = false)
{
    public bool HasExtra => !string.IsNullOrEmpty(Extra);
}

/// <summary>Ligne de deux cartes (grille 2 colonnes du bilan).</summary>
public sealed record TileRow(SummaryTile Left, SummaryTile Right);
