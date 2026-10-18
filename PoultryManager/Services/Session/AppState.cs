namespace PoultryManager.Services.Session;

/// <summary>
/// Informations transmises entre onglets pendant la session (en mémoire uniquement).
/// Exemple : « Voir le détail » sur le bilan d'une bande ouvre l'historique filtré sur cette bande.
/// </summary>
public sealed class AppState
{
    private int? _pendingHistoryBandId;
    private int? _pendingReportBandId;

    /// <summary>Demande l'ouverture de l'historique filtré sur une bande.</summary>
    public void RequestHistoryFor(int bandId) => _pendingHistoryBandId = bandId;

    /// <summary>Demande l'ouverture des rapports avec une bande présélectionnée.</summary>
    public void RequestReportFor(int bandId) => _pendingReportBandId = bandId;

    /// <summary>Lit puis efface la bande demandée pour l'historique.</summary>
    public int? TakeHistoryRequest()
    {
        var value = _pendingHistoryBandId;
        _pendingHistoryBandId = null;
        return value;
    }

    /// <summary>Lit puis efface la bande demandée pour les rapports.</summary>
    public int? TakeReportRequest()
    {
        var value = _pendingReportBandId;
        _pendingReportBandId = null;
        return value;
    }
}
