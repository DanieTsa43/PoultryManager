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

/// <summary>ViewModel de l'écran 05 · Production d'œufs.</summary>
public partial class EggEntryViewModel : DailyEntryViewModelBase
{
    private readonly IRecordRepository _records;
    private EggRecord? _existing;

    public EggEntryViewModel(IFarmService farmService, IRecordRepository records, INavigationService navigationService, ISettingsService settings)
        : base(farmService, navigationService, settings)
    {
        _records = records;
        Title = "Production d'œufs";
    }

    public IReadOnlyList<string> Categories => AppConstants.EggCategories;

    [ObservableProperty]
    private string eggsText = string.Empty;

    [ObservableProperty]
    private string brokenText = string.Empty;

    [ObservableProperty]
    private string? category = AppConstants.EggCategories[0];

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEggsError))]
    private string? eggsError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBrokenError))]
    private string? brokenError;

    public bool HasEggsError => EggsError is not null;
    public bool HasBrokenError => BrokenError is not null;

    partial void OnEggsTextChanged(string value) => EggsError = null;
    partial void OnBrokenTextChanged(string value) => BrokenError = null;

    protected override async Task LoadDayAsync()
    {
        _existing = SelectedBand is null ? null : await _records.GetEggRecordAsync(SelectedBand.Id, Date);
        HasExistingRecord = _existing is not null;
        EggsText = _existing is null ? string.Empty : Formats.Integer(_existing.EggsCollected);
        BrokenText = _existing?.BrokenEggs is int broken ? Formats.Integer(broken) : string.Empty;
        Category = string.IsNullOrEmpty(_existing?.Category) ? AppConstants.EggCategories[0] : _existing.Category;
        Notes = _existing?.Notes ?? string.Empty;
        EggsError = BrokenError = null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SuccessMessage = null;
        if (IsBusy || !ValidateCommon() || !Validate(out var eggs, out var broken))
        {
            return;
        }

        try
        {
            IsBusy = true;
            await _records.SaveEggRecordAsync(new EggRecord
            {
                Id = _existing?.Id ?? 0,
                BandId = SelectedBand!.Id,
                Date = Date.Date,
                EggsCollected = eggs,
                BrokenEggs = broken,
                Category = Category == AppConstants.EggCategories[^1] ? string.Empty : Category ?? string.Empty,
                Notes = Notes.Trim()
            });

            await ReloadAsync();
            var rate = Records?.DayStat(Date)?.LayingRate;
            SuccessMessage = rate.HasValue
                ? $"Suivi enregistré · taux de ponte {Formats.Percent(rate)}"
                : "Suivi enregistré";
        }
        catch (Exception)
        {
            FormError = "Le suivi n'a pas pu être enregistré. Veuillez réessayer.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool Validate(out int eggs, out int? broken)
    {
        broken = null;
        EggsError = BrokenError = null;
        var startCount = StartCount;

        if (!NumberParser.TryParseInt(EggsText, out eggs) || eggs < 0)
        {
            EggsError = "Indiquez le nombre d'œufs collectés (0 ou plus).";
        }
        else if (eggs > startCount)
        {
            EggsError = $"Le nombre d'œufs ne peut pas dépasser l'effectif de la bande ({Formats.Integer(startCount)}).";
        }

        if (!string.IsNullOrWhiteSpace(BrokenText))
        {
            if (!NumberParser.TryParseInt(BrokenText, out var value) || value < 0)
            {
                BrokenError = "Nombre invalide.";
            }
            else if (value > eggs)
            {
                BrokenError = "Les œufs cassés ne peuvent pas dépasser les œufs collectés.";
            }
            else
            {
                broken = value;
            }
        }

        return !HasEggsError && !HasBrokenError;
    }
}
