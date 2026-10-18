using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Helpers;
using PoultryManager.Services.Auth;
using PoultryManager.Services.Dialogs;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Settings;

namespace PoultryManager.ViewModels;

/// <summary>
/// ViewModel de l'écran 12 · Profil et paramètres (onglet Plus).
/// Chaque rubrique ouvre sa propre page.
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly ISettingsService _settings;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    public SettingsViewModel(
        IAuthService authService,
        ISettingsService settings,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _authService = authService;
        _settings = settings;
        _dialogService = dialogService;
        _navigationService = navigationService;
        Title = "Paramètres";
    }

    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private string initials = string.Empty;

    [ObservableProperty]
    private string roleText = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string farmText = string.Empty;

    [ObservableProperty]
    private string unitsText = string.Empty;

    [ObservableProperty]
    private string reminderText = string.Empty;

    [ObservableProperty]
    private string backupText = string.Empty;

    public override Task OnAppearingAsync()
    {
        Refresh();
        return Task.CompletedTask;
    }

    /// <summary>Met à jour les libellés à partir du profil et des réglages.</summary>
    public void Refresh()
    {
        var user = _authService.CurrentUser;
        FullName = user?.FullName ?? string.Empty;
        Initials = user?.Initials ?? string.Empty;
        Email = user?.Email ?? string.Empty;
        RoleText = string.IsNullOrWhiteSpace(user?.FarmName) ? "Éleveur" : $"Éleveur · {user.FarmName}";
        FarmText = string.IsNullOrWhiteSpace(user?.FarmName) ? "Non renseignée" : user.FarmName;

        var weight = _settings.EggWeightGrams;
        UnitsText = $"kg, g, % · poids moyen d'un œuf : {Formats.Decimal(weight, weight % 1 == 0 ? 0 : 1)} g";
        ReminderText = _settings.ReminderEnabled
            ? $"Rappel de saisie quotidien à {_settings.ReminderHour} h"
            : "Rappel désactivé";
        BackupText = _settings.LastBackupDate is DateTime last
            ? $"Dernière sauvegarde : {Formats.ShortDate(last)}"
            : "Aucune sauvegarde pour l'instant";
    }

    /// <summary>« Informations de l'exploitation » → page exploitation.</summary>
    [RelayCommand]
    private Task EditFarmAsync() => _navigationService.GoToAsync(Routes.Farm);

    /// <summary>« Profil utilisateur » → écran de modification du profil.</summary>
    [RelayCommand]
    private Task EditProfileAsync() => _navigationService.GoToAsync(Routes.ProfileEdit);

    /// <summary>« Unités de mesure » → page unités.</summary>
    [RelayCommand]
    private Task EditUnitsAsync() => _navigationService.GoToAsync(Routes.Units);

    /// <summary>« Notifications et rappels » → page rappels.</summary>
    [RelayCommand]
    private Task EditRemindersAsync() => _navigationService.GoToAsync(Routes.Reminders);

    /// <summary>« Sauvegarde des données » → page sauvegarde.</summary>
    [RelayCommand]
    private Task BackupAsync() => _navigationService.GoToAsync(Routes.Backup);

    /// <summary>« Déconnexion » → 00 · Bienvenue.</summary>
    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirmed = await _dialogService.ShowConfirmAsync(
            "Déconnexion", "Voulez-vous vous déconnecter ?", "Se déconnecter", "Annuler");
        if (!confirmed)
        {
            return;
        }

        _authService.Logout();
        await _navigationService.GoToAsync(Routes.WelcomePath);
    }

    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoToAsync(Routes.DashboardPath);
}
