using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Helpers;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Settings;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran « Unités de mesure » (depuis 12 · Paramètres).</summary>
public partial class UnitsViewModel : BaseViewModel
{
    public const double MinWeight = 30;
    public const double MaxWeight = 100;

    private readonly ISettingsService _settings;
    private readonly INavigationService _navigationService;

    public UnitsViewModel(ISettingsService settings, INavigationService navigationService)
    {
        _settings = settings;
        _navigationService = navigationService;
        Title = "Unités de mesure";
    }

    /// <summary>Poids moyen d'un œuf, en grammes (lié au contrôle « + / - »).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WeightText))]
    private double eggWeight;

    public string WeightText => $"{Formats.Decimal(EggWeight, EggWeight % 1 == 0 ? 0 : 1)} g";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSuccess))]
    private string? successMessage;

    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

    public override Task OnAppearingAsync()
    {
        EggWeight = _settings.EggWeightGrams;
        SuccessMessage = null;
        return Task.CompletedTask;
    }

    /// <summary>« Enregistrer les unités ».</summary>
    [RelayCommand]
    private void Save()
    {
        _settings.EggWeightGrams = Math.Clamp(Math.Round(EggWeight, 1), MinWeight, MaxWeight);
        EggWeight = _settings.EggWeightGrams;
        SuccessMessage = "Unités enregistrées";
    }

    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoBackAsync();
}
