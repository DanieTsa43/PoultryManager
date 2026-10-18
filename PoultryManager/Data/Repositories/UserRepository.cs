using PoultryManager.Models;

namespace PoultryManager.Data.Repositories;

/// <summary>Implémentation SQLite de <see cref="IUserRepository"/>.</summary>
public sealed class UserRepository : IUserRepository
{
    private readonly AppDatabase _database;

    public UserRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<int> CountAsync()
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<User>().CountAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<User>().Where(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<User>> FindByIdentifierAsync(string identifier)
    {
        var key = Normalize(identifier);
        var db = await _database.GetConnectionAsync();
        return await db.Table<User>().Where(u => u.Email == key || u.UserName == key).ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email, int? exceptUserId = null)
    {
        var key = Normalize(email);
        var except = exceptUserId ?? 0;
        var db = await _database.GetConnectionAsync();
        var count = await db.Table<User>().Where(u => u.Email == key && u.Id != except).CountAsync();
        return count > 0;
    }

    public async Task<int> AddAsync(User user)
    {
        user.Email = Normalize(user.Email);
        user.UserName = Normalize(user.UserName);
        var db = await _database.GetConnectionAsync();
        await db.InsertAsync(user);
        return user.Id;
    }

    public async Task UpdateAsync(User user)
    {
        user.Email = Normalize(user.Email);
        user.UserName = Normalize(user.UserName);
        var db = await _database.GetConnectionAsync();
        await db.UpdateAsync(user);
    }

    /// <summary>Les e-mails et noms d'utilisateur sont stockés en minuscules, sans espaces autour.</summary>
    private static string Normalize(string value) => value.Trim().ToLowerInvariant();
}
