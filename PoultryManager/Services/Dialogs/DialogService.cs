namespace PoultryManager.Services.Dialogs;

/// <summary>Implémentation basée sur les boîtes de dialogue de la page Shell courante.</summary>
public sealed class DialogService : IDialogService
{
    public Task ShowAlertAsync(string title, string message, string cancel = "OK")
        => Shell.Current?.DisplayAlert(title, message, cancel) ?? Task.CompletedTask;

    public Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
        => Shell.Current?.DisplayAlert(title, message, accept, cancel) ?? Task.FromResult(false);

    public async Task<string?> ShowActionSheetAsync(string title, string cancel, params string[] options)
    {
        if (Shell.Current is null)
        {
            return null;
        }

        var choice = await Shell.Current.DisplayActionSheet(title, cancel, null, options);
        return choice is null || choice == cancel ? null : choice;
    }

    public async Task<string?> ShowPromptAsync(string title, string message, string initialValue, bool numeric = false)
    {
        if (Shell.Current is null)
        {
            return null;
        }

        return await Shell.Current.DisplayPromptAsync(
            title,
            message,
            accept: "Enregistrer",
            cancel: "Annuler",
            maxLength: 80,
            keyboard: numeric ? Keyboard.Numeric : Keyboard.Text,
            initialValue: initialValue);
    }
}
