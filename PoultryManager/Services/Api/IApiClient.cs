namespace PoultryManager.Services.Api;

/// <summary>Appels HTTP vers l'API PoultryManager (JSON, jeton de connexion ajouté automatiquement).</summary>
public interface IApiClient
{
    /// <summary>GET ; renvoie la valeur par défaut (null / false) si le serveur répond 404.</summary>
    Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken = default);

    /// <summary>GET d'un fichier (sauvegarde).</summary>
    Task<byte[]> GetBytesAsync(string path, CancellationToken cancellationToken = default);

    Task<T?> PostAsync<T>(string path, object? body, CancellationToken cancellationToken = default);

    Task PostAsync(string path, object? body, CancellationToken cancellationToken = default);

    Task<T?> PutAsync<T>(string path, object? body, CancellationToken cancellationToken = default);

    /// <summary>DELETE ; renvoie faux si l'élément n'existe pas (404).</summary>
    Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default);
}
