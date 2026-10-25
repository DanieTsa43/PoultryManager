namespace PoultryManager.Services.Api;

/// <summary>
/// Erreur renvoyée par le serveur (ou serveur injoignable : <see cref="StatusCode"/> = 0).
/// <see cref="Exception.Message"/> est un texte en français, prêt à être affiché.
/// </summary>
public sealed class ApiException : Exception
{
    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>Code HTTP (400, 401, 409…), ou 0 si le serveur n'a pas pu être joint.</summary>
    public int StatusCode { get; }

    public bool IsNetworkError => StatusCode == 0;
}
