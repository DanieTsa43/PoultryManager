namespace PoultryManager.Services.Api;

/// <summary>
/// Conserve le jeton dans le stockage sécurisé de l'appareil (Keystore Android).
/// Si ce stockage n'est pas disponible (ex. application Windows non empaquetée), repli sur les préférences.
/// </summary>
public sealed class TokenStore : ITokenStore
{
    private const string Key = "session.api_token";

    private readonly IPreferences _preferences;

    public TokenStore(IPreferences preferences)
    {
        _preferences = preferences;
    }

    public string? Token { get; private set; }

    public async Task LoadAsync()
    {
        if (!string.IsNullOrEmpty(Token))
        {
            return;
        }

        string? stored = null;
        try
        {
            stored = await SecureStorage.Default.GetAsync(Key);
        }
        catch (Exception)
        {
            // Stockage sécurisé indisponible : on essaie les préférences.
        }

        if (string.IsNullOrEmpty(stored))
        {
            stored = _preferences.Get(Key, string.Empty);
        }

        Token = string.IsNullOrEmpty(stored) ? null : stored;
    }

    public async Task SetAsync(string token, bool persist)
    {
        Token = token;

        if (!persist)
        {
            RemoveStored();
            return;
        }

        try
        {
            await SecureStorage.Default.SetAsync(Key, token);
            _preferences.Remove(Key);
        }
        catch (Exception)
        {
            _preferences.Set(Key, token);
        }
    }

    public void Clear()
    {
        Token = null;
        RemoveStored();
    }

    private void RemoveStored()
    {
        try
        {
            SecureStorage.Default.Remove(Key);
        }
        catch (Exception)
        {
            // Rien à supprimer ou stockage indisponible.
        }

        _preferences.Remove(Key);
    }
}
