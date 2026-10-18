namespace PoultryManager.Services.Sharing;

/// <summary>Enregistrement d'un fichier puis ouverture du menu de partage du système
/// (e-mail, WhatsApp, Drive, enregistrement…).</summary>
public interface IFileShareService
{
    /// <summary>Écrit le fichier dans le dossier temporaire de l'application et le partage.</summary>
    Task ShareAsync(string fileName, byte[] content, string contentType, string title);

    /// <summary>Copie un fichier existant (ex. la base de données) puis le partage.</summary>
    Task ShareCopyAsync(string sourcePath, string fileName, string contentType, string title);
}
