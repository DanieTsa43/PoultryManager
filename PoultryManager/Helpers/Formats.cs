using System.Globalization;

namespace PoultryManager.Helpers;

/// <summary>
/// Mise en forme française des nombres et des dates (voir DESIGN.md) :
/// séparateur de milliers = espace insécable, décimales = virgule, dates JJ/MM/AAAA.
/// </summary>
public static class Formats
{
    /// <summary>Culture française dont le séparateur de milliers est une espace insécable classique (U+00A0),
    /// présente dans la police Plus Jakarta Sans.</summary>
    public static readonly CultureInfo French = CreateFrenchCulture();

    public const string Dash = "—";

    private static CultureInfo CreateFrenchCulture()
    {
        var culture = (CultureInfo)CultureInfo.GetCultureInfo("fr-FR").Clone();
        culture.NumberFormat.NumberGroupSeparator = " ";
        culture.NumberFormat.NumberDecimalSeparator = ",";
        culture.NumberFormat.PercentGroupSeparator = " ";
        return culture;
    }

    /// <summary>1532 → « 1 532 ».</summary>
    public static string Integer(int value) => value.ToString("N0", French);

    /// <summary>1532 → « 1 532 » ; null → « — ».</summary>
    public static string Integer(int? value) => value.HasValue ? Integer(value.Value) : Dash;

    /// <summary>87.54 → « 87,5 » (1 décimale par défaut).</summary>
    public static string Decimal(double value, int decimals = 1) => value.ToString("N" + decimals, French);

    /// <summary>null → « — ».</summary>
    public static string Decimal(double? value, int decimals = 1) => value.HasValue ? Decimal(value.Value, decimals) : Dash;

    /// <summary>87.5 → « 87,5 % ».</summary>
    public static string Percent(double? value, int decimals = 1) => value.HasValue ? Decimal(value.Value, decimals) + " %" : Dash;

    /// <summary>207 → « 207 kg » ; 9550 → « 9 550 kg ».</summary>
    public static string Kilograms(double? value)
    {
        if (!value.HasValue)
        {
            return Dash;
        }

        var decimals = Math.Abs(value.Value - Math.Round(value.Value)) < 0.05 ? 0 : 1;
        return Decimal(value.Value, decimals) + " kg";
    }

    /// <summary>118.24 → « 118,2 g ».</summary>
    public static string Grams(double? value) => value.HasValue ? Decimal(value.Value, 1) + " g" : Dash;

    /// <summary>Écart signé : +0,5 / −0,7.</summary>
    public static string SignedDecimal(double value, int decimals = 1)
        => (value >= 0 ? "+" : "−") + Decimal(Math.Abs(value), decimals);

    /// <summary>18/05/2025.</summary>
    public static string ShortDate(DateTime date) => date.ToString("dd/MM/yyyy", French);

    /// <summary>18/05.</summary>
    public static string DayMonth(DateTime date) => date.ToString("dd/MM", French);

    /// <summary>18 mai 2025.</summary>
    public static string LongDate(DateTime date) => date.ToString("d MMMM yyyy", French);

    /// <summary>« 1 mort » / « 2 morts ».</summary>
    public static string Plural(int count, string singular, string plural)
        => $"{Integer(count)} {(Math.Abs(count) > 1 ? plural : singular)}";
}
