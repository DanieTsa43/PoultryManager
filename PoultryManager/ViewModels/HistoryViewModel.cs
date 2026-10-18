using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Data.Repositories;
using PoultryManager.Helpers;
using PoultryManager.Models;
using PoultryManager.Services.Dialogs;
using PoultryManager.Services.Farm;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Session;
using PoultryManager.Services.Settings;
using PoultryManager.ViewModels.Items;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran 09 · Historique des suivis (onglet Suivi).</summary>
public partial class HistoryViewModel : BaseViewModel
{
    private const string EggsChoice = "Œufs";
    private const string FeedChoice = "Aliment";
    private const string MortalityChoice = "Mortalité";

    private readonly IFarmService _farmService;
    private readonly IRecordRepository _records;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ISettingsService _settings;
    private readonly AppState _appState;
    private int? _bandFilter;
    private int? _periodDays = 7;
    private IReadOnlyList<Band> _bands = [];

    public HistoryViewModel(
        IFarmService farmService,
        IRecordRepository records,
        INavigationService navigationService,
        IDialogService dialogService,
        ISettingsService settings,
        AppState appState)
    {
        _farmService = farmService;
        _records = records;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _settings = settings;
        _appState = appState;
        Title = "Historique";
        PeriodChips =
        [
            new OptionChip("7 jours", 7, SelectPeriod) { IsSelected = true },
            new OptionChip("30 jours", 30, SelectPeriod),
            new OptionChip("Tout", null, SelectPeriod)
        ];
    }

    public Func<DateTime> Clock { get; set; } = () => DateTime.Now;

    /// <summary>Filtres par bande : « Toutes », puis chaque bande.</summary>
    public ObservableCollection<OptionChip> BandChips { get; } = [];

    /// <summary>Filtres de période : 7 jours, 30 jours, tout.</summary>
    public IReadOnlyList<OptionChip> PeriodChips { get; }

    public ObservableCollection<HistoryDayItem> Items { get; } = [];

    [ObservableProperty]
    private string periodText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool hasItems;

    public bool IsEmpty => !HasItems && !IsBusy;

    public override async Task OnAppearingAsync()
    {
        _bands = await _farmService.GetBandsAsync();

        // Demande venant du bilan d'une bande (« Voir le détail »).
        if (_appState.TakeHistoryRequest() is int requested)
        {
            _bandFilter = requested;
        }

        if (_bandFilter is int id && _bands.All(b => b.Id != id))
        {
            _bandFilter = null;
        }

        BandChips.Clear();
        BandChips.Add(new OptionChip("Toutes", null, SelectBand) { IsSelected = _bandFilter is null });
        foreach (var band in _bands)
        {
            BandChips.Add(new OptionChip(band.Name, band.Id, SelectBand) { IsSelected = _bandFilter == band.Id });
        }

        await LoadAsync();
    }

    private void SelectBand(OptionChip chip)
    {
        foreach (var c in BandChips) c.IsSelected = c == chip;
        _bandFilter = chip.Value as int?;
        _ = LoadAsync();
    }

    private void SelectPeriod(OptionChip chip)
    {
        foreach (var c in PeriodChips) c.IsSelected = c == chip;
        _periodDays = chip.Value as int?;
        _ = LoadAsync();
    }

    /// <summary>Construit la liste des journées (les plus récentes d'abord).</summary>
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var today = Clock().Date;
            var bands = _bandFilter is int id ? _bands.Where(b => b.Id == id).ToList() : _bands.ToList();
            var rows = new List<(Band Band, DailyStat Stat)>();
            DateTime? earliest = null;

            foreach (var band in bands)
            {
                var records = await _farmService.GetBandRecordsAsync(band);
                var to = records.PeriodEnd(today);
                var from = _periodDays is int days ? today.AddDays(-(days - 1)) : band.StartDate;
                foreach (var stat in records.DailyStats(from, to).Where(s => s.HasAnyRecord))
                {
                    rows.Add((band, stat));
                }

                earliest = earliest is null || band.StartDate < earliest ? band.StartDate : earliest;
            }

            Items.Clear();
            var ordered = rows.OrderByDescending(r => r.Stat.Date).ThenBy(r => r.Band.Name).ToList();
            for (var i = 0; i < ordered.Count; i++)
            {
                Items.Add(new HistoryDayItem(ordered[i].Band, ordered[i].Stat, i == 0, EditDayAsync, DeleteDayAsync));
            }

            var periodStart = _periodDays is int d ? today.AddDays(-(d - 1)) : earliest ?? today;
            PeriodText = $"Période : {Formats.ShortDate(periodStart)} – {Formats.ShortDate(today)}";
            HasItems = Items.Count > 0;
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    /// <summary>« Modifier » : choix du type de saisie à corriger.</summary>
    private async Task EditDayAsync(HistoryDayItem item)
    {
        var choice = await _dialogService.ShowActionSheetAsync(
            $"Modifier le suivi du {item.DateText}", "Annuler", EggsChoice, FeedChoice, MortalityChoice);

        var route = choice switch
        {
            EggsChoice => Routes.EggEntry,
            FeedChoice => Routes.FeedEntry,
            MortalityChoice => Routes.MortalityEntry,
            _ => null
        };

        if (route is not null)
        {
            await _navigationService.GoToAsync(route, new Dictionary<string, object>
            {
                [Routes.BandIdParameter] = item.Band.Id,
                [Routes.DateParameter] = item.Stat.Date
            });
        }
    }

    /// <summary>« Supprimer » : supprime toutes les saisies du jour pour la bande.</summary>
    private async Task DeleteDayAsync(HistoryDayItem item)
    {
        var confirmed = await _dialogService.ShowConfirmAsync(
            "Supprimer ce suivi ?",
            $"Le suivi du {item.DateText} ({item.BandName}) sera supprimé. L'effectif et les indicateurs seront recalculés.",
            "Supprimer",
            "Annuler");

        if (!confirmed)
        {
            return;
        }

        await _records.DeleteDayAsync(item.Band.Id, item.Stat.Date);
        await LoadAsync();
    }

    /// <summary>Lien « Indicateurs » → 08, pour la bande filtrée (ou la bande du tableau de bord).</summary>
    [RelayCommand]
    private Task OpenIndicatorsAsync()
        => _navigationService.GoToAsync(Routes.Indicators, new Dictionary<string, object>
        {
            [Routes.BandIdParameter] = _bandFilter ?? _settings.SelectedBandId
        });

    /// <summary>Liste vide : bouton « Ajouter un suivi ».</summary>
    [RelayCommand]
    private Task AddEntryAsync()
        => _navigationService.GoToAsync(Routes.EggEntry, new Dictionary<string, object>
        {
            [Routes.BandIdParameter] = _bandFilter ?? _settings.SelectedBandId,
            [Routes.DateParameter] = Clock().Date
        });

    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoToAsync(Routes.DashboardPath);
}
