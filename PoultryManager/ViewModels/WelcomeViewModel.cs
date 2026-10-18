using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Startup;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran 00 · Bienvenue.</summary>
public partial class WelcomeViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IAppInitializer _appInitializer;

    public WelcomeViewModel(INavigationService navigationService, IAppInitializer appInitializer)
    {
        _navigationService = navigationService;
        _appInitializer = appInitializer;
        Title = "Bienvenue";
    }

    /// <summary>
    /// Appelé à l'affichage de l'écran : prépare la base de données et, si une session
    /// a été mémorisée (« Se souvenir de moi »), ouvre directement le tableau de bord.
    /// </summary>
    [RelayCommand]
    private async Task InitializeAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            if (await _appInitializer.InitializeAsync())
            {
                await _navigationService.GoToAsync(Routes.DashboardPath);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Bouton « Commencer » → 02 · Inscription.</summary>
    [RelayCommand]
    private Task StartAsync() => _navigationService.GoToAsync(Routes.Register);

    /// <summary>Bouton « J'ai déjà un compte » → 01 · Connexion.</summary>
    [RelayCommand]
    private Task GoToLoginAsync() => _navigationService.GoToAsync(Routes.Login);
}
