using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Services.Auth;
using PoultryManager.Services.Farm;
using PoultryManager.Services.Navigation;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran « Informations de l'exploitation » (depuis 12 · Paramètres).</summary>
public partial class FarmInfoViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IFarmService _farmService;
    private readonly INavigationService _navigationService;

    public FarmInfoViewModel(IAuthService authService, IFarmService farmService, INavigationService navigationService)
    {
        _authService = authService;
        _farmService = farmService;
        _navigationService = navigationService;
        Title = "Exploitation";
    }

    [ObservableProperty]
    private string farmName = string.Empty;

    /// <summary>Nom de l'éleveur titulaire du compte (lecture seule ici : il se modifie dans le profil).</summary>
    [ObservableProperty]
    private string ownerName = string.Empty;

    [ObservableProperty]
    private string bandsSummary = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSuccess))]
    private string? successMessage;

    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

    public override async Task OnAppearingAsync()
    {
        if (_authService.CurrentUser is { } user)
        {
            FarmName = user.FarmName;
            OwnerName = user.FullName;
        }

        var bands = await _farmService.GetBandsAsync(includeClosed: true);
        var active = bands.Count(b => !b.IsClosed);
        BandsSummary = $"{bands.Count} bande(s) enregistrée(s), dont {active} active(s)";
    }

    /// <summary>« Enregistrer l'exploitation ».</summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = SuccessMessage = null;
        var name = FarmName.Trim();
        if (name.Length < 2)
        {
            ErrorMessage = "Indiquez le nom de l'exploitation (2 caractères minimum).";
            return;
        }

        var user = _authService.CurrentUser;
        if (user is null)
        {
            ErrorMessage = "Aucun utilisateur connecté.";
            return;
        }

        var result = await _authService.UpdateProfileAsync(user.FullName, name, user.Phone, user.Email);
        if (result.Succeeded)
        {
            FarmName = name;
            SuccessMessage = "Exploitation enregistrée";
        }
        else
        {
            ErrorMessage = result.ErrorMessage;
        }
    }

    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoBackAsync();
}
