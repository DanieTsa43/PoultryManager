using PoultryManager.Helpers;
using PoultryManager.Models;
using PoultryManager.Services.Api;

namespace PoultryManager.Services.Auth;

/// <summary>
/// Authentification auprès du serveur (API ASP.NET Core). Le mot de passe n'est jamais conservé sur l'appareil :
/// seul le jeton remis par le serveur est mémorisé (si « Se souvenir de moi » est coché).
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IApiClient _api;
    private readonly ITokenStore _tokens;

    public AuthService(IApiClient api, ITokenStore tokens)
    {
        _api = api;
        _tokens = tokens;
    }

    public User? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public async Task<AuthResult> LoginAsync(string identifier, string password, bool rememberSession, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _api.PostAsync<LoginResponse>(
                "api/auth/login", new { identifier, password }, cancellationToken);
            return await OpenSessionAsync(response, rememberSession);
        }
        catch (ApiException ex)
        {
            return AuthResult.Failure(ex.Message);
        }
    }

    public async Task<AuthResult> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _api.PostAsync<LoginResponse>(
                "api/auth/register",
                new
                {
                    fullName = request.FullName.Trim(),
                    farmName = request.FarmName.Trim(),
                    phone = request.Phone.Trim(),
                    email = request.Email.Trim(),
                    password = request.Password
                },
                cancellationToken);
            return await OpenSessionAsync(response, rememberSession: true);
        }
        catch (ApiException ex)
        {
            return AuthResult.Failure(ex.Message);
        }
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        await _tokens.LoadAsync();
        if (string.IsNullOrEmpty(_tokens.Token))
        {
            return false;
        }

        try
        {
            var user = await _api.GetAsync<User>("api/auth/me");
            if (user is null)
            {
                return false;
            }

            CurrentUser = user;
            return true;
        }
        catch (ApiException ex)
        {
            // Jeton refusé (expiré, compte supprimé…) : on l'oublie. Serveur injoignable : on le garde pour la prochaine fois.
            if (ex.StatusCode == 401)
            {
                _tokens.Clear();
            }

            return false;
        }
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

        try
        {
            var saved = await _api.PutAsync<User>("api/auth/profile", new
            {
                fullName = fullName.Trim(),
                farmName = farmName.Trim(),
                phone = phone.Trim(),
                email = email.Trim()
            });

            if (saved is null)
            {
                return AuthResult.Failure("Réponse invalide du serveur.");
            }

            CurrentUser.FullName = saved.FullName;
            CurrentUser.FarmName = saved.FarmName;
            CurrentUser.Phone = saved.Phone;
            CurrentUser.Email = saved.Email;
            CurrentUser.UserName = saved.UserName;
            return AuthResult.Success(CurrentUser);
        }
        catch (ApiException ex)
        {
            return AuthResult.Failure(ex.Message);
        }
    }

    public async Task<AuthResult> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        if (CurrentUser is null)
        {
            return AuthResult.Failure("Aucun utilisateur connecté.");
        }

        try
        {
            await _api.PostAsync("api/auth/change-password", new { currentPassword, newPassword });
            return AuthResult.Success(CurrentUser);
        }
        catch (ApiException ex)
        {
            return AuthResult.Failure(ex.Message);
        }
    }

    public async Task<AuthResult> ResetPasswordAsync(string email, string newPassword)
    {
        try
        {
            await _api.PostAsync("api/auth/reset-password", new { email = email.Trim(), newPassword });

            // Le serveur ne renvoie pas d'utilisateur : on renvoie un succès « vide » (l'écran n'utilise que Succeeded).
            return AuthResult.Success(new User { Email = email.Trim() });
        }
        catch (ApiException ex)
        {
            return AuthResult.Failure(ex.Message);
        }
    }

    public void Logout()
    {
        CurrentUser = null;
        _tokens.Clear();
    }

    private async Task<AuthResult> OpenSessionAsync(LoginResponse? response, bool rememberSession)
    {
        if (response is null || string.IsNullOrEmpty(response.Token) || response.User is null)
        {
            return AuthResult.Failure("Réponse invalide du serveur.");
        }

        await _tokens.SetAsync(response.Token, rememberSession);
        CurrentUser = response.User;
        return AuthResult.Success(response.User);
    }

    /// <summary>Réponse du serveur à la connexion ou à l'inscription.</summary>
    private sealed record LoginResponse(string Token, User User);
}
