using System.Security.Cryptography;

namespace PoultryManager.Services.Security;

/// <summary>
/// Hachage des mots de passe avec PBKDF2 (SHA-256, sel aléatoire).
/// Seule l'empreinte est enregistrée en base, jamais le mot de passe.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    /// <summary>Calcule l'empreinte et le sel (en Base64) d'un mot de passe.</summary>
    public static (string Hash, string Salt) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    /// <summary>Vérifie un mot de passe saisi par rapport à l'empreinte enregistrée.</summary>
    public static bool Verify(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(storedSalt);
            var expected = Convert.FromBase64String(storedHash);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
