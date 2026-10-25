using System.Globalization;

namespace PoultryManager.Api.Services;

/// <summary>Règles et formats partagés par les contrôleurs (mêmes messages que l'application).</summary>
public static class FarmRules
{
    private static readonly NumberFormatInfo FrenchNumbers = new()
    {
        NumberGroupSeparator = " ",
        NumberDecimalSeparator = ","
    };

    /// <summary>
    /// Dernière date acceptée pour une saisie. L'application envoie la date locale de l'éleveur ;
    /// on tolère un jour d'avance sur l'heure du serveur (UTC) pour ne pas refuser « aujourd'hui »
    /// à un utilisateur situé à l'est du méridien de Greenwich.
    /// </summary>
    public static DateTime MaxAllowedDate => Today.AddDays(1);

    /// <summary>Jour courant du serveur (UTC), sans fuseau : les dates de l'application sont des jours « nus », sans heure.</summary>
    public static DateTime Today => DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Unspecified);

    public static string Int(int value) => value.ToString("N0", FrenchNumbers);

    public static string Date(DateTime value) => value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    /// <summary>Texte nettoyé et tronqué (évite les dépassements de colonne).</summary>
    public static string Clean(string? value, int maxLength)
    {
        var text = (value ?? string.Empty).Trim();
        return text.Length > maxLength ? text[..maxLength] : text;
    }
}
