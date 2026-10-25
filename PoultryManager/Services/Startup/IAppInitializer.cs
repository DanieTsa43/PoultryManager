namespace PoultryManager.Services.Startup;

/// <summary>Préparation de l'application au lancement.</summary>
public interface IAppInitializer
{
    /// <summary>
    /// Rouvre la session mémorisée (« Se souvenir de moi ») auprès du serveur.
    /// Renvoie vrai si un utilisateur est connecté.
    /// </summary>
    Task<bool> InitializeAsync();
}
