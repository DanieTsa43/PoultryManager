using CommunityToolkit.Mvvm.Input;
using PoultryManager.Helpers;
using PoultryManager.Models;

namespace PoultryManager.ViewModels.Items;

/// <summary>Carte d'une journée de suivi d'une bande (écran 09 · Historique).</summary>
public sealed class HistoryDayItem
{
    public HistoryDayItem(Band band, DailyStat stat, bool isLatest, Func<HistoryDayItem, Task> edit, Func<HistoryDayItem, Task> delete)
    {
        Band = band;
        Stat = stat;
        IsLatest = isLatest;
        EditCommand = new AsyncRelayCommand(() => edit(this));
        DeleteCommand = new AsyncRelayCommand(() => delete(this));
    }

    public Band Band { get; }

    public DailyStat Stat { get; }

    /// <summary>Saisie la plus récente (carte mise en avant).</summary>
    public bool IsLatest { get; }

    public string DateText => Formats.LongDate(Stat.Date);

    public string BandName => Band.Name;

    public string EggsText => Formats.Integer(Stat.Eggs);

    public string FeedText => Formats.Kilograms(Stat.FeedKg);

    public string DeathsText => Stat.HasMortalityRecord ? Formats.Integer(Stat.Deaths) : Formats.Dash;

    public string LayingRateText => Formats.Percent(Stat.LayingRate);

    public IAsyncRelayCommand EditCommand { get; }

    public IAsyncRelayCommand DeleteCommand { get; }
}
