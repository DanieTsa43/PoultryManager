using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Helpers;
using PoultryManager.Models;
using PoultryManager.Services.Auth;
using PoultryManager.Services.Dialogs;
using PoultryManager.Services.Farm;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Settings;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran 03 · Tableau de bord (onglet Accueil).</summary>
public partial class DashboardViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IFarmService _farmService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ISettingsService _settings;
    private bool _isLoadingBands;

    public DashboardViewModel(
        IAuthService authService,
        IFarmService farmService,
        INavigationService navigationService,
        IDialogService dialogService,
        ISettingsService settings)
    {
        _authService = authService;
        _farmService = farmService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _settings = settings;
        Title = "Tableau de bord";
    }

    /// <summary>Date du jour (remplaçable dans les tests).</summary>
    public Func<DateTime> Clock { get; set; } = () => DateTime.Now;

    // ---------- En-tête ----------

    [ObservableProperty]
    private string greeting = "Bonjour";

    [ObservableProperty]
    private string farmName = string.Empty;

    [ObservableProperty]
    private string initials = string.Empty;

    [ObservableProperty]
    private string todayText = string.Empty;

    // ---------- Bandes ----------

    /// <summary>Bandes actives proposées dans la liste déroulante.</summary>
    public ObservableCollection<Band> Bands { get; } = [];

    [ObservableProperty]
    private Band? selectedBand;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoBand))]
    private bool hasBand;

    public bool HasNoBand => !HasBand;

    // ---------- Indicateurs du jour ----------

    [ObservableProperty]
    private string currentCountText = Formats.Dash;

    [ObservableProperty]
    private string eggsTodayText = Formats.Dash;

    [ObservableProperty]
    private string feedTodayText = Formats.Dash;

    [ObservableProperty]
    private string deathsTodayText = Formats.Dash;

    [ObservableProperty]
    private string layingRateText = Formats.Dash;

    /// <summary>Écart avec la veille, ex. « +0,5 pt ».</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLayingRateDelta))]
    private string? layingRateDeltaText;

    public bool HasLayingRateDelta => !string.IsNullOrEmpty(LayingRateDeltaText);

    /// <summary>Œufs collectés sur les 7 derniers jours (graphique).</summary>
    [ObservableProperty]
    private IReadOnlyList<ChartPoint> productionPoints = [];

    // ---------- Rappel de saisie ----------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasReminder))]
    private string? reminderText;

    public bool HasReminder => !string.IsNullOrEmpty(ReminderText);

    // ---------- Chargement ----------

    public override async Task OnAppearingAsync()
    {
        var user = _authService.CurrentUser;
        Greeting = user is null ? "Bonjour" : $"Bonjour, {user.FirstName}";
        FarmName = user?.FarmName ?? string.Empty;
        Initials = user?.Initials ?? string.Empty;
        TodayText = $"Aujourd'hui · {Formats.LongDate(Clock())}";

        await LoadBandsAsync();
        await LoadSelectedBandAsync();
    }

    private async Task LoadBandsAsync()
    {
        _isLoadingBands = true;
        try
        {
            var bands = await _farmService.GetBandsAsync(includeClosed: false);
            Bands.Clear();
            foreach (var band in bands)
            {
                Bands.Add(band);
            }

            HasBand = Bands.Count > 0;
            SelectedBand = Bands.FirstOrDefault(b => b.Id == _settings.SelectedBandId) ?? Bands.FirstOrDefault();
        }
        finally
        {
            _isLoadingBands = false;
        }
    }

    partial void OnSelectedBandChanged(Band? value)
    {
        if (value is not null)
        {
            _settings.SelectedBandId = value.Id;
        }

        if (!_isLoadingBands)
        {
            _ = LoadSelectedBandAsync();
        }
    }

    /// <summary>Calcule les indicateurs du jour pour la bande choisie.</summary>
    public async Task LoadSelectedBandAsync()
    {
        var band = SelectedBand;
        if (band is null)
        {
            ResetIndicators();
            return;
        }

        var records = await _farmService.GetBandRecordsAsync(band);
        var today = Clock().Date;
        var days = records.DailyStats(today.AddDays(-6), today);
        var todayStat = days.LastOrDefault(d => d.Date == today);
        var yesterdayStat = days.LastOrDefault(d => d.Date == today.AddDays(-1));

        CurrentCountText = Formats.Integer(records.CurrentCount());
        EggsTodayText = Formats.Integer(todayStat?.Eggs);
        FeedTodayText = Formats.Kilograms(todayStat?.FeedKg);
        DeathsTodayText = todayStat?.HasMortalityRecord == true ? Formats.Integer(todayStat.Deaths) : Formats.Dash;
        LayingRateText = Formats.Percent(todayStat?.LayingRate);

        LayingRateDeltaText = todayStat?.LayingRate is double rate && yesterdayStat?.LayingRate is double previous
            ? Formats.SignedDecimal(rate - previous) + " pt"
            : null;

        ProductionPoints = days
            .Where(d => d.Eggs.HasValue)
            .Select(d => new ChartPoint(Formats.DayMonth(d.Date), d.Eggs!.Value, Formats.Integer(d.Eggs.Value), d.Date == today))
            .ToList();

        ReminderText = BuildReminder(band, todayStat);
    }

    /// <summary>Message de rappel si les saisies du jour manquent après l'heure choisie.</summary>
    private string? BuildReminder(Band band, DailyStat? todayStat)
    {
        if (!_settings.ReminderEnabled || Clock().Hour < _settings.ReminderHour)
        {
            return null;
        }

        var missing = new List<string>();
        if (todayStat?.Eggs is null) missing.Add("œufs");
        if (todayStat?.FeedKg is null) missing.Add("aliment");
        return missing.Count == 0
            ? null
            : $"Pensez à saisir les suivis du jour pour {band.Name} : {string.Join(" et ", missing)}.";
    }

    private void ResetIndicators()
    {
        CurrentCountText = EggsTodayText = FeedTodayText = DeathsTodayText = LayingRateText = Formats.Dash;
        LayingRateDeltaText = null;
        ProductionPoints = [];
        ReminderText = null;
    }

    // ---------- Commandes ----------

    private Dictionary<string, object> BandParameters() => new()
    {
        [Routes.BandIdParameter] = SelectedBand?.Id ?? 0,
        [Routes.DateParameter] = Clock().Date
    };

    /// <summary>Carte « Effectif actuel » → 04 · Bandes.</summary>
    [RelayCommand]
    private Task OpenBandsAsync() => _navigationService.GoToAsync(Routes.BandsPath);

    /// <summary>Carte « Œufs du jour » ou bouton « + Œufs » → 05.</summary>
    [RelayCommand]
    private Task AddEggsAsync() => HasBand ? _navigationService.GoToAsync(Routes.EggEntry, BandParameters()) : CreateBandAsync();

    /// <summary>Carte « Aliment du jour » ou bouton « + Aliment » → 06.</summary>
    [RelayCommand]
    private Task AddFeedAsync() => HasBand ? _navigationService.GoToAsync(Routes.FeedEntry, BandParameters()) : CreateBandAsync();

    /// <summary>Carte « Mortalité du jour » ou bouton « + Mortalité » → 07.</summary>
    [RelayCommand]
    private Task AddMortalityAsync() => HasBand ? _navigationService.GoToAsync(Routes.MortalityEntry, BandParameters()) : CreateBandAsync();

    /// <summary>Carte « Taux de ponte du jour » → 08 · Indicateurs.</summary>
    [RelayCommand]
    private Task OpenIndicatorsAsync() => HasBand ? _navigationService.GoToAsync(Routes.Indicators, BandParameters()) : Task.CompletedTask;

    /// <summary>Lien « Bilan » → 10 · Bilan de la bande.</summary>
    [RelayCommand]
    private Task OpenSummaryAsync() => HasBand ? _navigationService.GoToAsync(Routes.BandSummary, BandParameters()) : Task.CompletedTask;

    /// <summary>Avatar → 12 · Profil et paramètres.</summary>
    [RelayCommand]
    private Task OpenSettingsAsync() => _navigationService.GoToAsync(Routes.SettingsPath);

    /// <summary>Aucune bande : création de la première bande (04b).</summary>
    [RelayCommand]
    private Task CreateBandAsync()
        => _navigationService.GoToAsync(Routes.BandEdit, new Dictionary<string, object> { [Routes.BandIdParameter] = 0 });

    /// <summary>Cloche : liste des saisies manquantes du jour pour toutes les bandes actives.</summary>
    [RelayCommand]
    private async Task ShowNotificationsAsync()
    {
        var today = Clock().Date;
        var lines = new List<string>();
        foreach (var band in Bands)
        {
            var stat = (await _farmService.GetBandRecordsAsync(band)).DayStat(today);
            var missing = new List<string>();
            if (stat?.Eggs is null) missing.Add("œufs");
            if (stat?.FeedKg is null) missing.Add("aliment");
            if (stat?.HasMortalityRecord != true) missing.Add("mortalité");
            if (missing.Count > 0)
            {
                lines.Add($"• {band.Name} : {string.Join(", ", missing)}");
            }
        }

        var message = lines.Count == 0
            ? "Toutes les saisies du jour sont faites. Bravo !"
            : "Saisies du jour à compléter :\n" + string.Join("\n", lines) +
              "\n\n(La mortalité n'est à saisir que s'il y a des morts.)";
        await _dialogService.ShowAlertAsync("Notifications", message);
    }
}
