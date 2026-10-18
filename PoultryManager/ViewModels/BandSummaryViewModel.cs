using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Helpers;
using PoultryManager.Models;
using PoultryManager.Services.Farm;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Session;
using PoultryManager.ViewModels.Items;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran 10 · Bilan par bande.</summary>
public partial class BandSummaryViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IFarmService _farmService;
    private readonly INavigationService _navigationService;
    private readonly AppState _appState;
    private int _bandId;

    public BandSummaryViewModel(IFarmService farmService, INavigationService navigationService, AppState appState)
    {
        _farmService = farmService;
        _navigationService = navigationService;
        _appState = appState;
        Title = "Bilan";
    }

    public Func<DateTime> Clock { get; set; } = () => DateTime.Now;

    [ObservableProperty]
    private string periodText = string.Empty;

    /// <summary>Les 6 cartes du bilan.</summary>
    [ObservableProperty]
    private IReadOnlyList<SummaryTile> tiles = [];

    /// <summary>Les cartes regroupées par 2 pour l'affichage en grille.</summary>
    [ObservableProperty]
    private IReadOnlyList<TileRow> tileRows = [];

    /// <summary>Taux de ponte par semaine (S1, S2…).</summary>
    [ObservableProperty]
    private IReadOnlyList<ChartPoint> weeklyPoints = [];

    [ObservableProperty]
    private string weeklyCaption = "% par semaine";

    /// <summary>Dernier résultat calculé (utilisé par les tests).</summary>
    public BandSummary? Summary { get; private set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(Routes.BandIdParameter, out var id) && id is int bandId)
        {
            _bandId = bandId;
        }
    }

    public override async Task OnAppearingAsync()
    {
        var records = await _farmService.GetBandRecordsAsync(_bandId);
        if (records is null)
        {
            await _navigationService.GoBackAsync();
            return;
        }

        var s = records.Summary(Clock());
        Summary = s;
        Title = $"Bilan · {s.Band.Name}";
        PeriodText = $"Du {Formats.ShortDate(s.From)} au {Formats.ShortDate(s.To)} · {Formats.Plural(s.Days, "jour", "jours")}";

        Tiles =
        [
            new SummaryTile("Effectif initial", Formats.Integer(s.InitialCount)),
            new SummaryTile(s.Band.IsClosed ? "Effectif final" : "Effectif actuel", Formats.Integer(s.FinalCount)),
            new SummaryTile("Total d'œufs collectés", Formats.Integer(s.TotalEggs)),
            new SummaryTile("Aliment consommé", Formats.Kilograms(s.TotalFeedKg)),
            new SummaryTile("Mortalité cumulée", Formats.Integer(s.CumulativeDeaths), Formats.Percent(s.MortalityRate)),
            new SummaryTile("Taux moyen de ponte", Formats.Percent(s.AverageLayingRate), IsHighlighted: true)
        ];

        TileRows = Enumerable.Range(0, Tiles.Count / 2).Select(i => new TileRow(Tiles[2 * i], Tiles[2 * i + 1])).ToList();

        // Au-delà de 12 semaines, on affiche les 12 dernières pour garder des barres lisibles.
        var weeks = s.WeeklyRates.Where(w => w.LayingRate.HasValue).ToList();
        var shown = weeks.Skip(Math.Max(0, weeks.Count - 12)).ToList();
        WeeklyPoints = shown
            .Select((w, i) => new ChartPoint($"S{w.WeekNumber}", w.LayingRate!.Value,
                Formats.Decimal(w.LayingRate.Value, i == shown.Count - 1 ? 1 : 0),
                i == shown.Count - 1))
            .ToList();
        WeeklyCaption = weeks.Count > shown.Count ? "% par semaine · 12 dernières" : "% par semaine";
    }

    /// <summary>« Voir le détail » → onglet Suivi filtré sur cette bande.</summary>
    [RelayCommand]
    private Task ViewDetailAsync()
    {
        _appState.RequestHistoryFor(_bandId);
        return _navigationService.GoToAsync(Routes.HistoryPath);
    }

    /// <summary>« Exporter » → onglet Rapports avec cette bande.</summary>
    [RelayCommand]
    private Task ExportAsync()
    {
        _appState.RequestReportFor(_bandId);
        return _navigationService.GoToAsync(Routes.ReportsPath);
    }

    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoBackAsync();
}
