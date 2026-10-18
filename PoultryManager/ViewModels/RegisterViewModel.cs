using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Helpers;
using PoultryManager.Models;
using PoultryManager.Services.Auth;
using PoultryManager.Services.Navigation;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran 02 · Inscription.</summary>
public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    public RegisterViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Créer un compte";
    }

    // ---------- Champs ----------

    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private string farmName = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private bool acceptTerms;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordVisible))]
    private bool isPasswordHidden = true;

    public bool IsPasswordVisible => !IsPasswordHidden;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConfirmPasswordVisible))]
    private bool isConfirmPasswordHidden = true;

    public bool IsConfirmPasswordVisible => !IsConfirmPasswordHidden;

    // ---------- Erreurs (null = champ valide) ----------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFullNameError))]
    private string? fullNameError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFarmNameError))]
    private string? farmNameError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPhoneError))]
    private string? phoneError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEmailError))]
    private string? emailError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPasswordError))]
    private string? passwordError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConfirmPasswordError))]
    private string? confirmPasswordError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTermsError))]
    private string? termsError;

    /// <summary>Erreur générale (ex. e-mail déjà utilisé).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFormError))]
    private string? formError;

    public bool HasFullNameError => FullNameError is not null;
    public bool HasFarmNameError => FarmNameError is not null;
    public bool HasPhoneError => PhoneError is not null;
    public bool HasEmailError => EmailError is not null;
    public bool HasPasswordError => PasswordError is not null;
    public bool HasConfirmPasswordError => ConfirmPasswordError is not null;
    public bool HasTermsError => TermsError is not null;
    public bool HasFormError => FormError is not null;

    public string SubmitButtonText => IsBusy ? "Création en cours…" : "Créer mon compte";

    // Dès que l'utilisateur corrige un champ, son message disparaît.
    partial void OnFullNameChanged(string value) => FullNameError = null;
    partial void OnFarmNameChanged(string value) => FarmNameError = null;
    partial void OnPhoneChanged(string value) => PhoneError = null;
    partial void OnEmailChanged(string value) { EmailError = null; FormError = null; }
    partial void OnPasswordChanged(string value) => PasswordError = null;
    partial void OnConfirmPasswordChanged(string value) => ConfirmPasswordError = null;
    partial void OnAcceptTermsChanged(bool value) => TermsError = null;

    // ---------- Commandes ----------

    /// <summary>Bouton « Créer mon compte ».</summary>
    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy || !Validate())
        {
            return;
        }

        try
        {
            SetBusy(true);
            var result = await _authService.RegisterAsync(
                new RegistrationRequest(FullName, FarmName, Phone, Email, Password));

            if (!result.Succeeded)
            {
                EmailError = result.ErrorMessage;
                return;
            }

            Password = string.Empty;
            ConfirmPassword = string.Empty;

            // Compte créé → 03 · Tableau de bord.
            await _navigationService.GoToAsync(Routes.DashboardPath);
        }
        catch (Exception)
        {
            FormError = "Le compte n'a pas pu être créé. Veuillez réessayer.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    private void TogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;

    [RelayCommand]
    private void ToggleConfirmPasswordVisibility() => IsConfirmPasswordHidden = !IsConfirmPasswordHidden;

    [RelayCommand]
    private void ToggleAcceptTerms() => AcceptTerms = !AcceptTerms;

    /// <summary>Flèche retour.</summary>
    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoBackAsync();

    /// <summary>Lien « Se connecter » → 01 · Connexion.</summary>
    [RelayCommand]
    private Task GoToLoginAsync()
        => _navigationService.GoToAsync($"//{Routes.Welcome}/{Routes.Login}");

    // ---------- Validation ----------

    /// <summary>Vérifie tous les champs ; renvoie vrai si le formulaire est valide.</summary>
    public bool Validate()
    {
        FullNameError = string.IsNullOrWhiteSpace(FullName) ? "Indiquez votre nom complet." : null;
        FarmNameError = string.IsNullOrWhiteSpace(FarmName) ? "Indiquez le nom de votre exploitation." : null;
        PhoneError = Validation.IsValidPhone(Phone) ? null : "Numéro de téléphone invalide.";
        EmailError = string.IsNullOrWhiteSpace(Email)
            ? "Indiquez votre adresse e-mail."
            : Validation.IsValidEmail(Email) ? null : "Adresse e-mail invalide.";
        PasswordError = Password.Length < AppConstants.MinPasswordLength
            ? $"Le mot de passe doit contenir au moins {AppConstants.MinPasswordLength} caractères."
            : null;
        ConfirmPasswordError = ConfirmPassword != Password ? "Les mots de passe ne correspondent pas." : null;
        TermsError = AcceptTerms ? null : "Vous devez accepter les conditions d'utilisation.";

        return !(HasFullNameError || HasFarmNameError || HasPhoneError || HasEmailError ||
                 HasPasswordError || HasConfirmPasswordError || HasTermsError);
    }

    private void SetBusy(bool value)
    {
        IsBusy = value;
        OnPropertyChanged(nameof(SubmitButtonText));
    }
}
