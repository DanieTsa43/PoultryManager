namespace PoultryManager.Services.Backup;

/// <summary>Sauvegarde des données du compte (bandes et saisies) sous forme de fichier.</summary>
public interface IBackupService
{
    /// <summary>Demande au serveur l'export complet du compte (JSON).</summary>
    Task<byte[]> CreateBackupAsync();
}
