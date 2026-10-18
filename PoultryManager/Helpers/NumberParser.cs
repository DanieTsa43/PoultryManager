using System.Globalization;

namespace PoultryManager.Helpers;

/// <summary>
/// Lecture des nombres saisis par l'utilisateur : accepte « 1 532 », « 1532 », « 207,5 » ou « 207.5 ».
/// </summary>
public static class NumberParser
{
    public static bool TryParseInt(string? text, out int value)
    {
        value = 0;
        var cleaned = Clean(text);
        return cleaned.Length > 0 &&
               int.TryParse(cleaned, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseDouble(string? text, out double value)
    {
        value = 0;
        var cleaned = Clean(text).Replace(',', '.');
        return cleaned.Length > 0 &&
               double.TryParse(cleaned, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                   CultureInfo.InvariantCulture, out value);
    }

    /// <summary>Retire les espaces (y compris insécables) utilisés comme séparateurs de milliers.</summary>
    private static string Clean(string? text)
        => string.Concat((text ?? string.Empty).Where(c => !char.IsWhiteSpace(c) && c != ' ' && c != ' '));
}
