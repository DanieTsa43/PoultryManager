using PoultryManager.Data.Repositories;
using PoultryManager.Helpers;
using PoultryManager.Models;
using PoultryManager.Services.Security;

namespace PoultryManager.Services.Auth;

/// <summary>Authentification à partir des comptes enregistrés dans la base locale.</summary>
public sealed class AuthService : IAuthService
{
    /// <summary>Clé de stockage de la session mémorisée.</summary>
    private const string SessionUserIdKey = "session.user_id";

    private readonly IUserRepository _users;
    private readonly IPreferences _preferences;

    public AuthService(IUserRepository users, IPreferences preferences)
    {
        _users = users;
        _preferences = preferences;
    }

    public User? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public async Task<AuthResult> LoginAsync(string identifier, string password, bool rememberSession, CancellationToken cancellationToken = default)
    {
        var candidates = await _users.FindByIdentifierAsync(identifier);

        // Le calcul de l'empreinte est volontairement coûteux : il s'exécute hors du fil de l'interface.
        var user = await Task.Run(
            () => candidates.FirstOrDefault(u => PasswordHasher.Verify(password, u.PasswordHash, u.PasswordSalt)),
            cancellationToken);

        if (user is null)
        {
            return AuthResult.Failure("Identifiant ou mot de passe incorrect.");
        }

        OpenSession(user, rememberSession);
        return AuthResult.Success(user);
    }

    public async Task<AuthResult> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        if (await _users.EmailExistsAsync(email))
        {
            return AuthResult.Failure("Un compte existe déjà avec cette adresse e-mail.");
        }

        var (hash, salt) = await Task.Run(() => PasswordHasher.Hash(request.Password), cancellationToken);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            FarmName = request.FarmName.Trim(),
            Phone = request.Phone.Trim(),
            Email = email,
            UserName = email.Split('@')[0],
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.Now
        };

        await _users.AddAsync(user);
        OpenSession(user, rememberSession: true);
        return AuthResult.Success(user);
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var userId = _preferences.Get(SessionUserIdKey, 0);
        if (userId <= 0)
        {
            return false;
        }

        var user = await _users.GetByIdAsync(userId);
        if (user is null)
        {
            _preferences.Remove(SessionUserIdKey);
            return false;
        }

        CurrentUser = user;
        return true;
    }

    public async Task<AuthResult> UpdateProfileAsync(string fullName, string farmName, string phone, string email)
    {
        if (CurrentUser is null)
        {
            return AuthResult.Failure("Aucun utilisateur connecté.");
        }

        if (!Validation.IsValidEmail(email))
        {
            return AuthResult.Failure("Adresse e-mail invalide.");
        }

        if (await _users.EmailExistsAsync(email, CurrentUser.Id))
        {
            return AuthResult.Failure("Cette adresse e-mail est déjà utilisée par un autre compte.");
        }

        CurrentUser.FullName = fullName.Trim();
        CurrentUser.FarmName = farmName.Trim();
        CurrentUser.Phone = phone.Trim();
        CurrentUser.Email = email.Trim();
        CurrentUser.UserName = email.Trim().Split('@')[0];
        await _users.UpdateAsync(CurrentUser);
        return AuthResult.Success(CurrentUser);
    }

    public async Task<AuthResult> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        if (CurrentUser is null)
        {
            return AuthResult.Failure("Aucun utilisateur connecté.");
        }

        var user = CurrentUser;
        var valid = await Task.Run(() => PasswordHasher.Verify(currentPassword, user.PasswordHash, user.PasswordSalt));
        if (!valid)
        {
            return AuthResult.Failure("Le mot de passe actuel est incorrect.");
        }

        var (hash, salt) = await Task.Run(() => PasswordHasher.Hash(newPassword));
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        await _users.UpdateAsync(user);
        return AuthResult.Success(user);
    }

    public async Task<AuthResult> ResetPasswordAsync(string email, string newPassword)
    {
        var key = email.Trim();
        var user = (await _users.FindByIdentifierAsync(key))
            .FirstOrDefault(u => string.Equals(u.Email, key, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            return AuthResult.Failure("Aucun compte n'est associé à cette adresse e-mail.");
        }

        var (hash, salt) = await Task.Run(() => PasswordHasher.Hash(newPassword));
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        await _users.UpdateAsync(user);
        return AuthResult.Success(user);
    }

    public void Logout()
    {
        CurrentUser = null;
        _preferences.Remove(SessionUserIdKey);
    }

    private void OpenSession(User user, bool rememberSession)
    {
        CurrentUser = user;
        if (rememberSession)
        {
            _preferences.Set(SessionUserIdKey, user.Id);
        }
        else
        {
            _preferences.Remove(SessionUserIdKey);
        }
    }
}
