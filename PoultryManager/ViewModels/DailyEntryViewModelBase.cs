using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Helpers;
using PoultryManager.Models;
using PoultryManager.Services.Farm;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Settings;

namespace PoultryManager.ViewModels;

/// <summary>
/// Base commune des écrans de saisie quotidienne (05 · Œufs, 06 · Aliment, 07 · Mortalité) :
/// choix de la date et de la bande, chargement de la saisie existante du jour, bandeau de succès.
/// </summary>
public abstract partial class DailyEntryViewModelBase : BaseViewModel, IQueryAttributable
{
    protected readonly IFarmService FarmService;
    protected readonly INavigationService NavigationService;
    protected readonly ISettingsService Settings;
    private int? _requestedBandId;
    private DateTime? _requestedDate;
    private bool _isLoading;

    protected DailyEntryViewModelBase(IFarmService farmService, INavigationService navigationService, ISettingsService settings)
    {
        FarmService = farmService;
        NavigationService = navigationService;
        Settings = settings;
    }

    public Func<DateTime> Clock { get; set; } = () => DateTime.Now;

    /// <summary>Bandes actives proposées.</summary>
    public ObservableCollection<Band> Bands { get; } = [];

    [ObservableProperty]
    private Band? selectedBand;

    [ObservableProperty]
    private DateTime date = DateTime.Today;

    public DateTime MaximumDate => Clock().Date;

    /// <summary>Saisies de la bande choisie.</summary>
    protected BandRecords? Records { get; private set; }

    /// <summary>Vrai si une saisie existe déjà pour ce jour (le bouton la met à jour).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExistingRecordText))]
    private bool hasExistingRecord;

    public string ExistingRecordText => HasExistingRecord
        ? "Une saisie existe déjà pour ce jour : elle sera mise à jour."
        : string.Empty;

    // ---------- Messages ----------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSuccess))]
    private string? successMessage;

    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFormError))]
    private string? formError;

    public bool HasFormError => !string.IsNullOrEmpty(FormError);

    [ObservableProperty]
    private bool hasBands = true;

    // ---------- Chargement ----------

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(Routes.BandIdParameter, out var id) && id is int bandId && bandId > 0)
        {
            _requestedBandId = bandId;
        }

        if (query.TryGetValue(Routes.DateParameter, out var value) && value is DateTime requested)
        {
            _requestedDate = requested.Date;
        }
    }

    public override async Task OnAppearingAsync()
    {
        _isLoading = true;
        try
        {
            var bands = await FarmService.GetBandsAsync(includeClosed: false);

            // Une bande clôturée peut être corrigée si elle est demandée explicitement (depuis l'historique).
            if (_requestedBandId is int requested && bands.All(b => b.Id != requested)
                && await FarmService.GetBandAsync(requested) is Band closedBand)
            {
                bands = [.. bands, closedBand];
            }

            var previous = SelectedBand?.Id;
            Bands.Clear();
            foreach (var band in bands)
            {
                Bands.Add(band);
            }

            HasBands = Bands.Count > 0;
            var wantedId = _requestedBandId ?? previous ?? Settings.SelectedBandId;
            SelectedBand = Bands.FirstOrDefault(b => b.Id == wantedId) ?? Bands.FirstOrDefault();
            if (_requestedDate.HasValue)
            {
                Date = _requestedDate.Value;
            }

            _requestedBandId = null;
            _requestedDate = null;
        }
        finally
        {
            _isLoading = false;
        }

        await ReloadAsync();
    }

    partial void OnSelectedBandChanged(Band? value)
    {
        if (!_isLoading)
        {
            SuccessMessage = null;
            _ = ReloadAsync();
        }
    }

    partial void OnDateChanged(DateTime value)
    {
        if (!_isLoading)
        {
            SuccessMessage = null;
            _ = ReloadAsync();
        }
    }

    /// <summary>Recharge les saisies de la bande et le formulaire du jour choisi.</summary>
    protected async Task ReloadAsync()
    {
        FormError = null;
        Records = SelectedBand is null ? null : await FarmService.GetBandRecordsAsync(SelectedBand);
        await LoadDayAsync();
    }

    /// <summary>Remplit le formulaire avec la saisie existante du jour (ou le vide).</summary>
    protected abstract Task LoadDayAsync();

    /// <summary>Contrôles communs : bande choisie, date dans la période de la bande.</summary>
    protected bool ValidateCommon()
    {
        FormError = null;
        if (SelectedBand is null)
        {
            FormError = "Choisissez une bande.";
        }
        else if (Date.Date < SelectedBand.StartDate.Date)
        {
            FormError = $"La bande a démarré le {Formats.ShortDate(SelectedBand.StartDate)} : choisissez une date ultérieure.";
        }
        else if (Date.Date > Clock().Date)
        {
            FormError = "La date ne peut pas être dans le futur.";
        }
        else if (SelectedBand.IsClosed && SelectedBand.ClosedDate is DateTime closed && Date.Date > closed)
        {
            FormError = $"La bande a été clôturée le {Formats.ShortDate(closed)}.";
        }

        return FormError is null;
    }

    /// <summary>Effectif en début de journée pour la date choisie.</summary>
    protected int StartCount => Records?.CountAtStartOfDay(Date) ?? 0;

    /// <summary>Lien « historique » → onglet Suivi, filtré sur la bande.</summary>
    [RelayCommand]
    private Task OpenHistoryAsync() => NavigationService.GoToAsync(Routes.HistoryPath);

    [RelayCommand]
    private Task GoBackAsync() => NavigationService.GoBackAsync();

    /// <summary>Aucune bande active : création d'une bande.</summary>
    [RelayCommand]
    private Task CreateBandAsync()
        => NavigationService.GoToAsync(Routes.BandEdit, new Dictionary<string, object> { [Routes.BandIdParameter] = 0 });
}
