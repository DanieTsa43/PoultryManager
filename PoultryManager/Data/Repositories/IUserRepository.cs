using PoultryManager.Models;

namespace PoultryManager.Data.Repositories;

/// <summary>Comptes utilisateurs.</summary>
public interface IUserRepository
{
    Task<int> CountAsync();

    Task<User?> GetByIdAsync(int id);

    /// <summary>Recherche par adresse e-mail ou nom d'utilisateur (sans tenir compte des majuscules).</summary>
    Task<IReadOnlyList<User>> FindByIdentifierAsync(string identifier);

    /// <summary>Vrai si l'e-mail est déjà utilisé par un autre compte que <paramref name="exceptUserId"/>.</summary>
    Task<bool> EmailExistsAsync(string email, int? exceptUserId = null);

    /// <summary>Ajoute le compte et renvoie son identifiant.</summary>
    Task<int> AddAsync(User user);

    Task UpdateAsync(User user);
}
