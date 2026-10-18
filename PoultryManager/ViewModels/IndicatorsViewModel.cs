using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Helpers;
using PoultryManager.Models;
using PoultryManager.Services.Farm;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Settings;
using PoultryManager.Services.Statistics;
using PoultryManager.ViewModels.Items;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran 08 · Indicateurs de rendement.</summary>
public partial class IndicatorsViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IFarmService _farmService;
    private readonly INavigationService _navigationService;
    private readonly ISettingsService _settings;
    private int? _requestedBandId;
    private int? _periodDays = 7;
    private bool _isLoading;

    public IndicatorsViewModel(IFarmService farmService, INavigationService navigationService, ISettingsService settings)
    {
        _farmService = farmService;
        _navigationService = navigationService;
        _settings = settings;
        Title = "Indicateurs de rendement";
        PeriodChips =
        [
            new OptionChip("7 j", 7, SelectPeriod) { IsSelected = true },
            new OptionChip("30 j", 30, SelectPeriod),
            new OptionChip("Bande", null, SelectPeriod)
        ];
    }

    public Func<DateTime> Clock { get; set; } = () => DateTime.Now;

    public ObservableCollection<Band> Bands { get; } = [];

    [ObservableProperty]
    private Band? selectedBand;

    public IReadOnlyList<OptionChip> PeriodChips { get; }

    // ---------- Cartes ----------

    [ObservableProperty]
    private string layingRateText = Formats.Dash;

    [ObservableProperty]
    private string layingRateDelta = string.Empty;

    [ObservableProperty]
    private string feedPerHenText = Formats.Dash;

    [ObservableProperty]
    private string feedPerHenDelta = string.Empty;

    [ObservableProperty]
    private string conversionText = Formats.Dash;

    [ObservableProperty]
    private string conversionCaption = string.Empty;

    [ObservableProperty]
    private string lastDayText = string.Empty;

    // ---------- Graphiques ----------

    [ObservableProperty]
    private IReadOnlyList<ChartPoint> productionPoints = [];

    [ObservableProperty]
    private IReadOnlyList<ChartPoint> mortalityPoints = [];

    [ObservableProperty]
    private string mortalityCaption = string.Empty;

    [ObservableProperty]
    private bool hasData;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(Routes.BandIdParameter, out var id) && id is int bandId && bandId > 0)
        {
            _requestedBandId = bandId;
        }
    }

    public override async Task OnAppearingAsync()
    {
        _isLoading = true;
        try
        {
            var bands = await _farmService.GetBandsAsync();
            var previous = SelectedBand?.Id;
            Bands.Clear();
            foreach (var band in bands)
            {
                Bands.Add(band);
            }

            var wanted = _requestedBandId ?? previous ?? _settings.SelectedBandId;
            SelectedBand = Bands.FirstOrDefault(b => b.Id == wanted) ?? Bands.FirstOrDefault();
            _requestedBandId = null;
        }
        finally
        {
            _isLoading = false;
        }

        await LoadAsync();
    }

    partial void OnSelectedBandChanged(Band? value)
    {
        if (!_isLoading)
        {
            _ = LoadAsync();
        }
    }

    private void SelectPeriod(OptionChip chip)
    {
        foreach (var c in PeriodChips) c.IsSelected = c == chip;
        _periodDays = chip.Value as int?;
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        var band = SelectedBand;
        if (band is null)
        {
            HasData = false;
            return;
        }

        var records = await _farmService.GetBandRecordsAsync(band);
        var to = records.PeriodEnd(Clock());
        var from = _periodDays is int days ? to.AddDays(-(days - 1)) : band.StartDate;
        var stats = records.DailyStats(from, to);
        HasData = stats.Any(s => s.HasAnyRecord);

        // Taux de ponte : dernier jour saisi, comparé au jour saisi précédent.
        var laying = stats.Where(s => s.LayingRate.HasValue).ToList();
        LayingRateText = Formats.Percent(laying.LastOrDefault()?.LayingRate);
        LayingRateDelta = laying.Count >= 2
            ? Arrow(laying[^1].LayingRate!.Value - laying[^2].LayingRate!.Value) + " pt depuis la veille"
            : string.Empty;

        var feeding = stats.Where(s => s.FeedPerHenGrams.HasValue).ToList();
        FeedPerHenText = Formats.Grams(feeding.LastOrDefault()?.FeedPerHenGrams);
        FeedPerHenDelta = feeding.Count >= 2
            ? Arrow(feeding[^1].FeedPerHenGrams!.Value - feeding[^2].FeedPerHenGrams!.Value) + " g depuis la veille"
            : string.Empty;

        // Indice de conversion du dernier jour ayant les deux saisies.
        var eggWeight = _settings.EggWeightGrams;
        var both = stats.LastOrDefault(s => s.Eggs > 0 && s.FeedKg > 0);
        ConversionText = Formats.Decimal(FarmCalculator.FeedConversionRatio(both?.FeedKg, both?.Eggs, eggWeight), 2);
        ConversionCaption = $"kg d'aliment ÷ kg d'œufs · poids moyen d'un œuf : {Formats.Decimal(eggWeight, eggWeight % 1 == 0 ? 0 : 1)} g";
        LastDayText = laying.Count > 0 ? $"Dernière saisie : {Formats.LongDate(laying[^1].Date)}" : "Aucune saisie sur la période";

        var lastDay = stats.LastOrDefault()?.Date;
        ProductionPoints = stats
            .Where(s => s.Eggs.HasValue)
            .Select(s => new ChartPoint(Formats.DayMonth(s.Date), s.Eggs!.Value, Formats.Integer(s.Eggs.Value), s.Date == lastDay))
            .ToList();

        MortalityPoints = stats
            .Select(s => new ChartPoint(Formats.DayMonth(s.Date), s.Deaths, s.Deaths > 0 ? Formats.Integer(s.Deaths) : null))
            .ToList();

        var deaths = stats.Sum(s => s.Deaths);
        MortalityCaption = $"{Formats.Plural(deaths, "mort", "morts")} en {Formats.Plural(stats.Count, "jour", "jours")}";
    }

    private static string Arrow(double delta) => (delta >= 0 ? "▲ " : "▼ ") + Formats.SignedDecimal(delta);

    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoBackAsync();
}
