namespace PoultryManager.Services.Startup;

/// <summary>Préparation de l'application au lancement.</summary>
public interface IAppInitializer
{
    /// <summary>
    /// Crée la base si besoin, ajoute le compte de démonstration au premier lancement
    /// et rouvre la session mémorisée. Renvoie vrai si un utilisateur est connecté.
    /// </summary>
    Task<bool> InitializeAsync();
}
