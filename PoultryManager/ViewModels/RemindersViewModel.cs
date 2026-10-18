using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Settings;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran « Notifications et rappels » (depuis 12 · Paramètres).</summary>
public partial class RemindersViewModel : BaseViewModel
{
    private readonly ISettingsService _settings;
    private readonly INavigationService _navigationService;

    public RemindersViewModel(ISettingsService settings, INavigationService navigationService)
    {
        _settings = settings;
        _navigationService = navigationService;
        Title = "Notifications et rappels";
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Summary))]
    private bool reminderEnabled;

    /// <summary>Heure du rappel (lié au contrôle « + / - », de 6 h à 22 h).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HourText))]
    [NotifyPropertyChangedFor(nameof(Summary))]
    private double reminderHour;

    public string HourText => $"{(int)Math.Round(ReminderHour):00} h 00";

    public string Summary => ReminderEnabled
        ? $"Rappel affiché chaque jour à {HourText} sur l'accueil, tant que les saisies du jour ne sont pas faites."
        : "Aucun rappel de saisie.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSuccess))]
    private string? successMessage;

    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

    public override Task OnAppearingAsync()
    {
        ReminderEnabled = _settings.ReminderEnabled;
        ReminderHour = _settings.ReminderHour;
        SuccessMessage = null;
        return Task.CompletedTask;
    }

    /// <summary>« Enregistrer les rappels ».</summary>
    [RelayCommand]
    private void Save()
    {
        _settings.ReminderEnabled = ReminderEnabled;
        _settings.ReminderHour = (int)Math.Round(ReminderHour);
        SuccessMessage = "Rappels enregistrés";
    }

    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoBackAsync();
}
