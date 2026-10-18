using PoultryManager.Models;

namespace PoultryManager.Data.Repositories;

/// <summary>Bandes de poules.</summary>
public interface IBandRepository
{
    /// <summary>Bandes de l'utilisateur : actives d'abord, puis par date de démarrage décroissante.</summary>
    Task<IReadOnlyList<Band>> GetBandsAsync(int userId);

    Task<Band?> GetByIdAsync(int bandId);

    /// <summary>Vrai si une autre bande de l'utilisateur porte déjà ce nom.</summary>
    Task<bool> NameExistsAsync(int userId, string name, int? exceptBandId = null);

    /// <summary>Ajoute la bande et renvoie son identifiant.</summary>
    Task<int> AddAsync(Band band);

    Task UpdateAsync(Band band);

    /// <summary>Supprime la bande et toutes ses saisies.</summary>
    Task DeleteAsync(int bandId);
}
