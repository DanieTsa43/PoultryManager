using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Data.Repositories;
using PoultryManager.Helpers;
using PoultryManager.Services.Dialogs;
using PoultryManager.Services.Farm;
using PoultryManager.Services.Navigation;
using PoultryManager.ViewModels.Items;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran 04 · Gestion des bandes (onglet Bandes).</summary>
public partial class BandsViewModel : BaseViewModel
{
    private readonly IFarmService _farmService;
    private readonly IBandRepository _bandRepository;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    public BandsViewModel(
        IFarmService farmService,
        IBandRepository bandRepository,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _farmService = farmService;
        _bandRepository = bandRepository;
        _navigationService = navigationService;
        _dialogService = dialogService;
        Title = "Mes bandes";
    }

    public Func<DateTime> Clock { get; set; } = () => DateTime.Now;

    public ObservableCollection<BandItem> Items { get; } = [];

    /// <summary>« 3 actives · 1 clôturée ».</summary>
    [ObservableProperty]
    private string countText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoBand))]
    private bool hasBands;

    public bool HasNoBand => !HasBands;

    public override async Task OnAppearingAsync()
    {
        var bands = await _farmService.GetBandsAsync();
        Items.Clear();
        foreach (var band in bands)
        {
            var records = await _farmService.GetBandRecordsAsync(band);
            Items.Add(new BandItem(band, records.CurrentCount(), OpenSummaryAsync, EditAsync, CloseAsync));
        }

        var active = bands.Count(b => !b.IsClosed);
        var closed = bands.Count - active;
        CountText = $"{Formats.Plural(active, "active", "actives")} · {Formats.Plural(closed, "clôturée", "clôturées")}";
        HasBands = Items.Count > 0;
    }

    /// <summary>Flèche retour → Accueil.</summary>
    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoToAsync(Routes.DashboardPath);

    /// <summary>Bouton « Nouvelle bande » → 04b.</summary>
    [RelayCommand]
    private Task NewBandAsync()
        => _navigationService.GoToAsync(Routes.BandEdit, new Dictionary<string, object> { [Routes.BandIdParameter] = 0 });

    /// <summary>Carte ou bouton « Bilan » → 10.</summary>
    private Task OpenSummaryAsync(BandItem item)
        => _navigationService.GoToAsync(Routes.BandSummary, new Dictionary<string, object> { [Routes.BandIdParameter] = item.Band.Id });

    /// <summary>Bouton « Modifier » → 04b en modification.</summary>
    private Task EditAsync(BandItem item)
        => _navigationService.GoToAsync(Routes.BandEdit, new Dictionary<string, object> { [Routes.BandIdParameter] = item.Band.Id });

    /// <summary>Bouton « Clôturer » : la bande passe en « Clôturée » à la date du jour.</summary>
    private async Task CloseAsync(BandItem item)
    {
        var confirmed = await _dialogService.ShowConfirmAsync(
            "Clôturer la bande ?",
            $"{item.Name} sera clôturée à la date du {Formats.ShortDate(Clock())}. " +
            "Elle n'apparaîtra plus dans les saisies quotidiennes, mais son bilan restera consultable.",
            "Clôturer",
            "Annuler");

        if (!confirmed)
        {
            return;
        }

        item.Band.IsClosed = true;
        item.Band.ClosedDate = Clock().Date;
        await _bandRepository.UpdateAsync(item.Band);
        await OnAppearingAsync();
    }
}
