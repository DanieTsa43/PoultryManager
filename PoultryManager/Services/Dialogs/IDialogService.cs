namespace PoultryManager.Services.Dialogs;

/// <summary>Affichage de messages et de choix à l'utilisateur depuis les ViewModels.</summary>
public interface IDialogService
{
    Task ShowAlertAsync(string title, string message, string cancel = "OK");

    /// <summary>Demande une confirmation ; renvoie vrai si l'utilisateur accepte.</summary>
    Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel);

    /// <summary>Liste de choix ; renvoie le libellé choisi, ou null si annulé.</summary>
    Task<string?> ShowActionSheetAsync(string title, string cancel, params string[] options);

    /// <summary>Saisie d'un texte ; renvoie null si annulé.</summary>
    Task<string?> ShowPromptAsync(string title, string message, string initialValue, bool numeric = false);
}
