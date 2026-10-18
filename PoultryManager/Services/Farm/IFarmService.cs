using PoultryManager.Models;

namespace PoultryManager.Services.Farm;

/// <summary>Accès aux bandes de l'utilisateur connecté et à leurs saisies.</summary>
public interface IFarmService
{
    /// <summary>Bandes de l'utilisateur connecté (actives d'abord).</summary>
    Task<IReadOnlyList<Band>> GetBandsAsync(bool includeClosed = true);

    Task<Band?> GetBandAsync(int bandId);

    /// <summary>Une bande et toutes ses saisies, prêtes pour les calculs.</summary>
    Task<BandRecords> GetBandRecordsAsync(Band band);

    /// <summary>Raccourci : saisies de la bande d'identifiant <paramref name="bandId"/> (null si introuvable).</summary>
    Task<BandRecords?> GetBandRecordsAsync(int bandId);
}
