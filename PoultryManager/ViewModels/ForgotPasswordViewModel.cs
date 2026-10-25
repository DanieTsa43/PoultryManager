using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Helpers;
using PoultryManager.Services.Auth;
using PoultryManager.Services.Navigation;

namespace PoultryManager.ViewModels;

/// <summary>
/// ViewModel de l'écran « Mot de passe oublié » (depuis 01 · Connexion).
/// La réinitialisation se fait auprès du serveur, en indiquant l'adresse e-mail du compte
/// (voir la remarque de sécurité dans AuthController.ResetPassword côté API).
/// </summary>
public partial class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    public ForgotPasswordViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Mot de passe oublié";
    }

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>Vrai une fois le mot de passe remplacé : le formulaire est remplacé par la confirmation.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowForm))]
    private bool isReset;

    public bool ShowForm => !IsReset;

    /// <summary>« Réinitialiser le mot de passe ».</summary>
    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        ErrorMessage = null;
        if (!Validation.IsValidEmail(Email))
        {
            ErrorMessage = "Adresse e-mail invalide.";
            return;
        }

        if (NewPassword.Length < AppConstants.MinPasswordLength)
        {
            ErrorMessage = $"Le nouveau mot de passe doit contenir au moins {AppConstants.MinPasswordLength} caractères.";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Les mots de passe ne correspondent pas.";
            return;
        }

        var result = await _authService.ResetPasswordAsync(Email, NewPassword);
        if (result.Succeeded)
        {
            NewPassword = ConfirmPassword = string.Empty;
            IsReset = true;
        }
        else
        {
            ErrorMessage = result.ErrorMessage;
        }
    }

    /// <summary>« Retour à la connexion ».</summary>
    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoBackAsync();
}
