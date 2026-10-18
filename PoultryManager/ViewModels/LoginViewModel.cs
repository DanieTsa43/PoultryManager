using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Services.Auth;
using PoultryManager.Services.Dialogs;
using PoultryManager.Services.Navigation;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran 01 · Connexion.</summary>
public partial class LoginViewModel : BaseViewModel
{
    /// <summary>
    /// Clé de stockage de l'identifiant mémorisé. Avec « Se souvenir de moi », la session est aussi
    /// rouverte automatiquement au prochain lancement (voir AuthService).
    /// </summary>
    private const string RememberedIdentifierKey = "login.remembered_identifier";

    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IPreferences _preferences;

    public LoginViewModel(
        IAuthService authService,
        INavigationService navigationService,
        IDialogService dialogService,
        IPreferences preferences)
    {
        _authService = authService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _preferences = preferences;

        Title = "Connexion";

        // Pré-remplit l'identifiant si l'utilisateur a coché « Se souvenir de moi ».
        var remembered = _preferences.Get(RememberedIdentifierKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(remembered))
        {
            Identifier = remembered;
            RememberMe = true;
        }
    }

    // ---------- Champs du formulaire ----------

    /// <summary>Adresse e-mail ou nom d'utilisateur.</summary>
    [ObservableProperty]
    private string identifier = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool rememberMe = true;

    /// <summary>Vrai : le mot de passe est masqué (●●●●).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordVisible))]
    private bool isPasswordHidden = true;

    public bool IsPasswordVisible => !IsPasswordHidden;

    // ---------- Erreurs ----------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>Le champ identifiant est affiché en rouge.</summary>
    [ObservableProperty]
    private bool hasIdentifierError;

    /// <summary>Le champ mot de passe est affiché en rouge.</summary>
    [ObservableProperty]
    private bool hasPasswordError;

    /// <summary>Libellé du bouton principal (état « chargement » de la maquette).</summary>
    public string LoginButtonText => IsBusy ? "Connexion en cours…" : "Se connecter";

    // Dès que l'utilisateur corrige un champ, son erreur disparaît.
    partial void OnIdentifierChanged(string value)
    {
        HasIdentifierError = false;
        ClearMessageIfNoError();
    }

    partial void OnPasswordChanged(string value)
    {
        HasPasswordError = false;
        ClearMessageIfNoError();
    }

    // ---------- Commandes ----------

    /// <summary>Bouton « Se connecter » (et touche Entrée du clavier).</summary>
    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy || !Validate())
        {
            return;
        }

        try
        {
            SetBusy(true);

            var result = await _authService.LoginAsync(Identifier.Trim(), Password, RememberMe);

            if (!result.Succeeded)
            {
                HasIdentifierError = true;
                HasPasswordError = true;
                ErrorMessage = result.ErrorMessage;
                return;
            }

            SaveRememberMeChoice();
            Password = string.Empty;

            // Connexion réussie → 03 · Tableau de bord (la pile de navigation est réinitialisée).
            await _navigationService.GoToAsync(Routes.DashboardPath);
        }
        catch (Exception)
        {
            ErrorMessage = "Une erreur inattendue est survenue. Veuillez réessayer.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Icône « œil » : affiche ou masque le mot de passe.</summary>
    [RelayCommand]
    private void TogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;

    /// <summary>Appui sur le texte « Se souvenir de moi ».</summary>
    [RelayCommand]
    private void ToggleRememberMe() => RememberMe = !RememberMe;

    /// <summary>Lien « Mot de passe oublié ? » → écran de réinitialisation.</summary>
    [RelayCommand]
    private Task ForgotPasswordAsync()
        => _navigationService.GoToAsync(Routes.ForgotPassword);

    /// <summary>Lien « Créer un compte » → 02 · Inscription.</summary>
    [RelayCommand]
    private Task GoToRegisterAsync()
        => _navigationService.GoToAsync($"//{Routes.Welcome}/{Routes.Register}");

    // ---------- Méthodes internes ----------

    private bool Validate()
    {
        HasIdentifierError = string.IsNullOrWhiteSpace(Identifier);
        HasPasswordError = string.IsNullOrEmpty(Password);

        ErrorMessage = (HasIdentifierError, HasPasswordError) switch
        {
            (true, true) => "Veuillez saisir votre identifiant et votre mot de passe.",
            (true, false) => "Veuillez saisir votre adresse e-mail ou votre nom d'utilisateur.",
            (false, true) => "Veuillez saisir votre mot de passe.",
            _ => null
        };

        return ErrorMessage is null;
    }

    private void ClearMessageIfNoError()
    {
        if (!HasIdentifierError && !HasPasswordError)
        {
            ErrorMessage = null;
        }
    }

    private void SaveRememberMeChoice()
    {
        if (RememberMe)
        {
            _preferences.Set(RememberedIdentifierKey, Identifier.Trim());
        }
        else
        {
            _preferences.Remove(RememberedIdentifierKey);
        }
    }

    private void SetBusy(bool value)
    {
        IsBusy = value;
        OnPropertyChanged(nameof(LoginButtonText));
    }
}
