namespace PoultryManager.Services.Api;

/// <summary>Jeton de connexion remis par le serveur : en mémoire, et sur l'appareil si « Se souvenir de moi ».</summary>
public interface ITokenStore
{
    /// <summary>Jeton courant, ou null si personne n'est connecté.</summary>
    string? Token { get; }

    /// <summary>Recharge le jeton mémorisé sur l'appareil (au lancement de l'application).</summary>
    Task LoadAsync();

    /// <summary>Retient le jeton ; <paramref name="persist"/> = vrai pour le conserver après fermeture de l'application.</summary>
    Task SetAsync(string token, bool persist);

    /// <summary>Oublie le jeton (déconnexion).</summary>
    void Clear();
}
