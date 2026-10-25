namespace PoultryManager.Constants;

/// <summary>Adresse du serveur (API ASP.NET Core PoultryManager.Api).</summary>
public static class ApiSettings
{
    /// <summary>Port de l'API en développement (voir PoultryManager.Api/Properties/launchSettings.json).</summary>
    public const int DevelopmentPort = 5080;

    /// <summary>
    /// Adresse à renseigner pour tester sur un VRAI téléphone (ou en production), ex. « http://192.168.1.20:5080 »
    /// ou « https://api.mon-domaine.com ». Laisser vide pour l'émulateur Android et Windows.
    /// </summary>
    public const string BaseUrlOverride = "";

    /// <summary>
    /// Émulateur Android : « 10.0.2.2 » désigne le PC qui héberge l'émulateur.
    /// Windows : l'API tourne sur la même machine (« localhost »).
    /// </summary>
    public static string BaseUrl
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(BaseUrlOverride))
            {
                return BaseUrlOverride.TrimEnd('/') + "/";
            }

            var host = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
            return $"http://{host}:{DevelopmentPort}/";
        }
    }

    /// <summary>Délai maximal d'attente d'une réponse du serveur.</summary>
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);
}
