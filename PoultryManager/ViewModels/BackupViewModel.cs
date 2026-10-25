using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Helpers;
using PoultryManager.Services.Backup;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Settings;
using PoultryManager.Services.Sharing;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran « Sauvegarde des données » (depuis 12 · Paramètres).</summary>
public partial class BackupViewModel : BaseViewModel
{
    private readonly ISettingsService _settings;
    private readonly IFileShareService _fileShareService;
    private readonly IBackupService _backupService;
    private readonly INavigationService _navigationService;

    public BackupViewModel(
        ISettingsService settings,
        IFileShareService fileShareService,
        IBackupService backupService,
        INavigationService navigationService)
    {
        _settings = settings;
        _fileShareService = fileShareService;
        _backupService = backupService;
        _navigationService = navigationService;
        Title = "Sauvegarde des données";
    }

    /// <summary>Date du jour (remplaçable dans les tests).</summary>
    public Func<DateTime> Clock { get; set; } = () => DateTime.Now;

    [ObservableProperty]
    private string lastBackupText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSuccess))]
    private string? successMessage;

    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

    public override Task OnAppearingAsync()
    {
        Refresh();
        return Task.CompletedTask;
    }

    private void Refresh()
    {
        LastBackupText = _settings.LastBackupDate is DateTime last
            ? $"Dernière sauvegarde : {Formats.ShortDate(last)}"
            : "Aucune sauvegarde pour l'instant";
    }

    /// <summary>« Créer une sauvegarde » : fichier d'export du compte, partagé (Drive, e-mail, clé USB…).</summary>
    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        ErrorMessage = SuccessMessage = null;
        try
        {
            var now = Clock();
            var content = await _backupService.CreateBackupAsync();
            await _fileShareService.ShareAsync(
                $"PoultryManager-sauvegarde-{now:yyyy-MM-dd}.json",
                content,
                "application/json",
                "Sauvegarde PoultryManager");
            _settings.LastBackupDate = now;
            Refresh();
            SuccessMessage = "Sauvegarde créée";
        }
        catch (Exception)
        {
            ErrorMessage = "La sauvegarde n'a pas pu être créée. Réessayez.";
        }
    }

    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoBackAsync();
}
