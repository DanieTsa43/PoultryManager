using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Data.Repositories;
using PoultryManager.Helpers;
using PoultryManager.Models;
using PoultryManager.Services.Farm;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Settings;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran 06 · Consommation d'aliment.</summary>
public partial class FeedEntryViewModel : DailyEntryViewModelBase
{
    private readonly IRecordRepository _records;
    private FeedRecord? _existing;

    public FeedEntryViewModel(IFarmService farmService, IRecordRepository records, INavigationService navigationService, ISettingsService settings)
        : base(farmService, navigationService, settings)
    {
        _records = records;
        Title = "Consommation d'aliment";
    }

    public IReadOnlyList<string> FeedTypes => AppConstants.FeedTypes;

    [ObservableProperty]
    private string? feedType = AppConstants.FeedTypes[0];

    [ObservableProperty]
    private string quantityText = string.Empty;

    [ObservableProperty]
    private string remainingText = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasQuantityError))]
    private string? quantityError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRemainingError))]
    private string? remainingError;

    public bool HasQuantityError => QuantityError is not null;
    public bool HasRemainingError => RemainingError is not null;

    // ---------- Carte calculée « Consommation par poule » ----------

    [ObservableProperty]
    private string perHenText = Formats.Dash;

    [ObservableProperty]
    private string perHenFormula = "Saisissez la quantité distribuée.";

    // ---------- Lien vers l'historique ----------

    [ObservableProperty]
    private string previousDayText = string.Empty;

    partial void OnQuantityTextChanged(string value)
    {
        QuantityError = null;
        UpdatePerHen();
    }

    partial void OnRemainingTextChanged(string value) => RemainingError = null;

    protected override async Task LoadDayAsync()
    {
        _existing = SelectedBand is null ? null : await _records.GetFeedRecordAsync(SelectedBand.Id, Date);
        HasExistingRecord = _existing is not null;
        FeedType = string.IsNullOrEmpty(_existing?.FeedType) ? AppConstants.FeedTypes[0] : _existing.FeedType;
        QuantityText = _existing is null ? string.Empty : Formats.Decimal(_existing.QuantityKg, _existing.QuantityKg % 1 == 0 ? 0 : 1);
        RemainingText = _existing?.RemainingKg is double remaining ? Formats.Decimal(remaining, remaining % 1 == 0 ? 0 : 1) : string.Empty;
        Notes = _existing?.Notes ?? string.Empty;
        QuantityError = RemainingError = null;

        var previous = Records?.DayStat(Date.AddDays(-1));
        PreviousDayText = previous?.FeedKg is double kg
            ? $"Veille : {Formats.Kilograms(kg)} · {Formats.Grams(previous.FeedPerHenGrams)} par poule"
            : "Aucune distribution saisie la veille";
        UpdatePerHen();
    }

    /// <summary>Consommation par poule = kg × 1 000 ÷ effectif vivant moyen du jour.</summary>
    private void UpdatePerHen()
    {
        var average = Records?.DayStat(Date)?.AverageCount;
        if (!NumberParser.TryParseDouble(QuantityText, out var kg) || kg <= 0 || average is not > 0)
        {
            PerHenText = Formats.Dash;
            PerHenFormula = average is > 0
                ? "Saisissez la quantité distribuée."
                : "Effectif nul : la consommation ne peut pas être calculée.";
            return;
        }

        PerHenText = Formats.Grams(kg * 1000 / average.Value);
        PerHenFormula = $"{Formats.Kilograms(kg)} ÷ {Formats.Decimal(average.Value, average.Value % 1 == 0 ? 0 : 1)} poules (effectif moyen du jour)";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SuccessMessage = null;
        if (IsBusy || !ValidateCommon() || !Validate(out var quantity, out var remaining))
        {
            return;
        }

        try
        {
            IsBusy = true;
            await _records.SaveFeedRecordAsync(new FeedRecord
            {
                Id = _existing?.Id ?? 0,
                BandId = SelectedBand!.Id,
                Date = Date.Date,
                FeedType = FeedType ?? string.Empty,
                QuantityKg = quantity,
                RemainingKg = remaining,
                Notes = Notes.Trim()
            });

            await ReloadAsync();
            var perHen = Records?.DayStat(Date)?.FeedPerHenGrams;
            SuccessMessage = perHen.HasValue
                ? $"Distribution enregistrée · {Formats.Grams(perHen)} par poule"
                : "Distribution enregistrée";
        }
        catch (Exception)
        {
            FormError = "La distribution n'a pas pu être enregistrée. Veuillez réessayer.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool Validate(out double quantity, out double? remaining)
    {
        remaining = null;
        QuantityError = RemainingError = null;

        if (!NumberParser.TryParseDouble(QuantityText, out quantity) || quantity <= 0)
        {
            QuantityError = "Indiquez la quantité distribuée en kg (supérieure à 0).";
        }
        else if (quantity > 100_000)
        {
            QuantityError = "Quantité trop élevée : vérifiez l'unité (kg).";
        }

        if (!string.IsNullOrWhiteSpace(RemainingText))
        {
            if (!NumberParser.TryParseDouble(RemainingText, out var value) || value < 0)
            {
                RemainingError = "Quantité invalide.";
            }
            else if (value > quantity)
            {
                RemainingError = "Le reste ne peut pas dépasser la quantité distribuée.";
            }
            else
            {
                remaining = value;
            }
        }

        return !HasQuantityError && !HasRemainingError;
    }
}
