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
using PoultryManager.Services.Settings;
using PoultryManager.ViewModels.Items;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran 07 · Mortalité.</summary>
public partial class MortalityEntryViewModel : DailyEntryViewModelBase
{
    private readonly IRecordRepository _records;
    private readonly IDialogService _dialogService;
    private MortalityRecord? _existing;

    public MortalityEntryViewModel(
        IFarmService farmService,
        IRecordRepository records,
        INavigationService navigationService,
        ISettingsService settings,
        IDialogService dialogService)
        : base(farmService, navigationService, settings)
    {
        _records = records;
        _dialogService = dialogService;
        Title = "Mortalité";
    }

    public IReadOnlyList<string> Causes => AppConstants.MortalityCauses;

    [ObservableProperty]
    private string deathsText = string.Empty;

    [ObservableProperty]
    private string? cause = AppConstants.MortalityCauses[0];

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDeathsError))]
    private string? deathsError;

    public bool HasDeathsError => DeathsError is not null;

    // ---------- Carte calculée « Effectif restant après saisie » ----------

    [ObservableProperty]
    private string remainingCountText = Formats.Dash;

    [ObservableProperty]
    private string remainingFormula = string.Empty;

    /// <summary>Dernières saisies de la bande (5 plus récentes).</summary>
    public ObservableCollection<MortalityItem> RecentItems { get; } = [];

    [ObservableProperty]
    private bool hasRecentItems;

    partial void OnDeathsTextChanged(string value)
    {
        DeathsError = null;
        UpdateRemaining();
    }

    protected override async Task LoadDayAsync()
    {
        _existing = SelectedBand is null ? null : await _records.GetMortalityRecordAsync(SelectedBand.Id, Date);
        HasExistingRecord = _existing is not null;
        DeathsText = _existing is null ? string.Empty : Formats.Integer(_existing.Deaths);
        Cause = string.IsNullOrEmpty(_existing?.Cause) ? AppConstants.MortalityCauses[0] : _existing.Cause;
        Notes = _existing?.Notes ?? string.Empty;
        DeathsError = null;
        UpdateRemaining();

        RecentItems.Clear();
        foreach (var record in (Records?.Mortality ?? []).OrderByDescending(m => m.Date).Take(5))
        {
            RecentItems.Add(new MortalityItem(record, EditRecentAsync, DeleteRecentAsync));
        }

        HasRecentItems = RecentItems.Count > 0;
    }

    /// <summary>Effectif restant = effectif en début de journée − morts saisis.</summary>
    private void UpdateRemaining()
    {
        if (Records is null)
        {
            RemainingCountText = Formats.Dash;
            RemainingFormula = string.Empty;
            return;
        }

        var start = StartCount;
        var deaths = NumberParser.TryParseInt(DeathsText, out var value) && value >= 0 ? value : 0;
        RemainingCountText = Formats.Integer(start - deaths);
        RemainingFormula = $"{Formats.Integer(start)} en début de journée − {Formats.Plural(deaths, "mort", "morts")}";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SuccessMessage = null;
        if (IsBusy || !ValidateCommon() || !Validate(out var deaths))
        {
            return;
        }

        try
        {
            IsBusy = true;
            await _records.SaveMortalityRecordAsync(new MortalityRecord
            {
                Id = _existing?.Id ?? 0,
                BandId = SelectedBand!.Id,
                Date = Date.Date,
                Deaths = deaths,
                Cause = Cause ?? string.Empty,
                Notes = Notes.Trim()
            });

            await ReloadAsync();
            SuccessMessage = $"Mortalité enregistrée · effectif {Formats.Integer(Records?.CurrentCount())}";
        }
        catch (Exception)
        {
            FormError = "La saisie n'a pas pu être enregistrée. Veuillez réessayer.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool Validate(out int deaths)
    {
        DeathsError = null;
        var start = StartCount;
        if (!NumberParser.TryParseInt(DeathsText, out deaths) || deaths < 0)
        {
            DeathsError = "Indiquez le nombre de morts (0 ou plus).";
        }
        else if (deaths > start)
        {
            DeathsError = $"Le nombre de morts ne peut pas dépasser l'effectif du jour ({Formats.Integer(start)}).";
        }

        return !HasDeathsError;
    }

    /// <summary>Crayon d'une ligne : charge la saisie dans le formulaire.</summary>
    private Task EditRecentAsync(MortalityItem item)
    {
        SuccessMessage = null;
        Date = item.Record.Date;
        return Task.CompletedTask;
    }

    /// <summary>Corbeille d'une ligne : suppression après confirmation.</summary>
    private async Task DeleteRecentAsync(MortalityItem item)
    {
        var confirmed = await _dialogService.ShowConfirmAsync(
            "Supprimer cette saisie ?",
            $"La saisie du {Formats.ShortDate(item.Record.Date)} ({Formats.Plural(item.Record.Deaths, "mort", "morts")}) sera supprimée. " +
            "L'effectif et les indicateurs seront recalculés.",
            "Supprimer",
            "Annuler");

        if (!confirmed)
        {
            return;
        }

        await _records.DeleteMortalityRecordAsync(item.Record.Id);
        SuccessMessage = null;
        await ReloadAsync();
    }
}
