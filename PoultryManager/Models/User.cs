namespace PoultryManager.Models;

/// <summary>Utilisateur de l'application (éleveur).</summary>
public sealed class User
{
    public int Id { get; set; }

    /// <summary>Nom complet, ex. « Jean Dupont ».</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Nom de l'exploitation, ex. « Ferme des Ailes ».</summary>
    public string FarmName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Nom d'utilisateur (partie de l'e-mail avant « @ »), utilisable pour se connecter.</summary>
    public string UserName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    /// <summary>Empreinte du mot de passe (PBKDF2). Le mot de passe n'est jamais stocké en clair.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string PasswordSalt { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    /// <summary>Prénom (premier mot du nom complet).</summary>
    public string FirstName =>
        FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? FullName;

    /// <summary>Initiales affichées dans l'avatar, ex. « JD ».</summary>
    public string Initials =>
        string.Concat(FullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0])));
}
