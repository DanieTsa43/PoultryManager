using PoultryManager.Models;
using SQLite;

namespace PoultryManager.Data;

/// <summary>
/// Accès à la base locale SQLite (fichier <c>poultrymanager.db3</c> dans le dossier de l'application).
/// Les tables sont créées automatiquement à la première utilisation.
/// </summary>
public sealed class AppDatabase
{
    private const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _connection;

    public AppDatabase(string databasePath)
    {
        DatabasePath = databasePath;
    }

    /// <summary>Chemin complet du fichier de base de données.</summary>
    public string DatabasePath { get; }

    /// <summary>Renvoie la connexion, en créant les tables si nécessaire.</summary>
    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
        {
            return _connection;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_connection is null)
            {
                var connection = new SQLiteAsyncConnection(DatabasePath, Flags);
                await connection.CreateTableAsync<User>();
                await connection.CreateTableAsync<Band>();
                await connection.CreateTableAsync<EggRecord>();
                await connection.CreateTableAsync<FeedRecord>();
                await connection.CreateTableAsync<MortalityRecord>();
                _connection = connection;
            }
        }
        finally
        {
            _initLock.Release();
        }

        return _connection;
    }
}
