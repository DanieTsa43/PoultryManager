using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Helpers;
using PoultryManager.Services.Auth;
using PoultryManager.Services.Navigation;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran « Profil utilisateur » (depuis 12 · Paramètres).</summary>
public partial class ProfileEditViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private bool _loaded;

    public ProfileEditViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Profil utilisateur";
    }

    // ---------- Profil ----------

    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProfileError))]
    private string? profileError;

    public bool HasProfileError => !string.IsNullOrEmpty(ProfileError);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProfileSuccess))]
    private string? profileSuccess;

    public bool HasProfileSuccess => !string.IsNullOrEmpty(ProfileSuccess);

    // ---------- Mot de passe ----------

    [ObservableProperty]
    private string currentPassword = string.Empty;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPasswordError))]
    private string? passwordError;

    public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPasswordSuccess))]
    private string? passwordSuccess;

    public bool HasPasswordSuccess => !string.IsNullOrEmpty(PasswordSuccess);

    public override Task OnAppearingAsync()
    {
        if (!_loaded && _authService.CurrentUser is { } user)
        {
            FullName = user.FullName;
            Phone = user.Phone;
            Email = user.Email;
            _loaded = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>« Enregistrer le profil ».</summary>
    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        ProfileError = ProfileSuccess = null;
        if (string.IsNullOrWhiteSpace(FullName))
        {
            ProfileError = "Indiquez votre nom complet.";
            return;
        }

        if (!Validation.IsValidPhone(Phone))
        {
            ProfileError = "Numéro de téléphone invalide.";
            return;
        }

        var user = _authService.CurrentUser;
        var result = await _authService.UpdateProfileAsync(FullName, user?.FarmName ?? string.Empty, Phone, Email);
        if (result.Succeeded)
        {
            ProfileSuccess = "Profil enregistré";
        }
        else
        {
            ProfileError = result.ErrorMessage;
        }
    }

    /// <summary>« Changer le mot de passe ».</summary>
    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        PasswordError = PasswordSuccess = null;
        if (NewPassword.Length < AppConstants.MinPasswordLength)
        {
            PasswordError = $"Le nouveau mot de passe doit contenir au moins {AppConstants.MinPasswordLength} caractères.";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            PasswordError = "Les mots de passe ne correspondent pas.";
            return;
        }

        var result = await _authService.ChangePasswordAsync(CurrentPassword, NewPassword);
        if (result.Succeeded)
        {
            PasswordSuccess = "Mot de passe modifié";
            CurrentPassword = NewPassword = ConfirmPassword = string.Empty;
        }
        else
        {
            PasswordError = result.ErrorMessage;
        }
    }

    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoBackAsync();
}
