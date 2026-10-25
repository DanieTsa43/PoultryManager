using PoultryManager.Services.Auth;

namespace PoultryManager.Services.Startup;

/// <summary>
/// Implémentation de <see cref="IAppInitializer"/>. La base de données et les données de démonstration
/// sont désormais gérées par le serveur (API) : au lancement, il suffit de rouvrir la session mémorisée.
/// </summary>
public sealed class AppInitializer : IAppInitializer
{
    private readonly IAuthService _authService;

    public AppInitializer(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> InitializeAsync()
        => _authService.IsAuthenticated || await _authService.TryRestoreSessionAsync();
}
