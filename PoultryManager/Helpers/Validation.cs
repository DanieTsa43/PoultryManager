using System.Text.RegularExpressions;

namespace PoultryManager.Helpers;

/// <summary>Règles de validation partagées par les formulaires.</summary>
public static partial class Validation
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    public static bool IsValidEmail(string? email)
        => !string.IsNullOrWhiteSpace(email) && EmailRegex().IsMatch(email.Trim());

    /// <summary>Téléphone facultatif : chiffres, espaces, « + », « - », « . » ; au moins 6 chiffres.</summary>
    public static bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return true;
        }

        var digits = phone.Count(char.IsDigit);
        return digits >= 6 && phone.All(c => char.IsDigit(c) || c is ' ' or '+' or '-' or '.' or '(' or ')');
    }
}
