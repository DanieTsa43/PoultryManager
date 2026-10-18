namespace PoultryManager.Models;

/// <summary>Point d'un graphique (courbe ou barre).</summary>
/// <param name="Label">Libellé sous l'axe (ex. « 18/05 », « S7 »).</param>
/// <param name="Value">Valeur numérique.</param>
/// <param name="ValueLabel">Texte affiché au-dessus de la barre ou du point (facultatif).</param>
/// <param name="Highlight">Vrai pour mettre ce point en évidence (dernier jour, dernière semaine…).</param>
public sealed record ChartPoint(string Label, double Value, string? ValueLabel = null, bool Highlight = false);
