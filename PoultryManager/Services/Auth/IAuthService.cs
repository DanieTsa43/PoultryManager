using PoultryManager.Models;

namespace PoultryManager.Services.Auth;

/// <summary>Authentification et session de l'utilisateur.</summary>
public interface IAuthService
{
    /// <summary>Utilisateur connecté, ou null.</summary>
    User? CurrentUser { get; }

    bool IsAuthenticated { get; }

    /// <summary>
    /// Vérifie les identifiants. <paramref name="identifier"/> peut être l'adresse e-mail ou le nom d'utilisateur.
    /// Si <paramref name="rememberSession"/> est vrai, la session est rouverte automatiquement au prochain lancement.
    /// </summary>
    Task<AuthResult> LoginAsync(string identifier, string password, bool rememberSession, CancellationToken cancellationToken = default);

    /// <summary>Crée un compte puis connecte l'utilisateur.</summary>
    Task<AuthResult> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);

    /// <summary>Rouvre la session mémorisée, si elle existe. Renvoie vrai si l'utilisateur est connecté.</summary>
    Task<bool> TryRestoreSessionAsync();

    /// <summary>Enregistre les modifications du profil de l'utilisateur connecté.</summary>
    Task<AuthResult> UpdateProfileAsync(string fullName, string farmName, string phone, string email);

    /// <summary>
    /// Remplace le mot de passe du compte portant l'adresse e-mail <paramref name="email"/>.
    /// L'application étant locale (sans serveur), la réinitialisation se fait sur l'appareil.
    /// </summary>
    Task<AuthResult> ResetPasswordAsync(string email, string newPassword);

    /// <summary>Change le mot de passe de l'utilisateur connecté.</summary>
    Task<AuthResult> ChangePasswordAsync(string currentPassword, string newPassword);

    void Logout();
}
