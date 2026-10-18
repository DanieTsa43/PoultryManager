using CommunityToolkit.Mvvm.Input;
using PoultryManager.Helpers;
using PoultryManager.Models;

namespace PoultryManager.ViewModels.Items;

/// <summary>Carte d'une bande dans la liste de l'écran 04 · Gestion des bandes.</summary>
public sealed class BandItem
{
    public BandItem(Band band, int currentCount, Func<BandItem, Task> openSummary, Func<BandItem, Task> edit, Func<BandItem, Task> close)
    {
        Band = band;
        CurrentCount = currentCount;
        OpenSummaryCommand = new AsyncRelayCommand(() => openSummary(this));
        EditCommand = new AsyncRelayCommand(() => edit(this));
        CloseCommand = new AsyncRelayCommand(() => close(this));
    }

    public Band Band { get; }

    public int CurrentCount { get; }

    public string Name => Band.Name;

    /// <summary>« Souche légère · démarrée le 01/04/2025 ».</summary>
    public string Subtitle
    {
        get
        {
            var strain = string.IsNullOrWhiteSpace(Band.Strain) ? string.Empty : Band.Strain + " · ";
            var closed = Band.IsClosed && Band.ClosedDate.HasValue ? $" · clôturée le {Formats.ShortDate(Band.ClosedDate.Value)}" : string.Empty;
            return $"{strain}démarrée le {Formats.ShortDate(Band.StartDate)}{closed}";
        }
    }

    public string InitialCountText => Formats.Integer(Band.InitialCount);

    public string CurrentCountText => Formats.Integer(CurrentCount);

    /// <summary>« Effectif actuel » ou « Effectif final » pour une bande clôturée.</summary>
    public string CurrentCountLabel => Band.IsClosed ? "Effectif final" : "Effectif actuel";

    public bool IsClosed => Band.IsClosed;

    public bool IsActive => !Band.IsClosed;

    public string StatusText => Band.IsClosed ? "Clôturée" : "Active";

    public IAsyncRelayCommand OpenSummaryCommand { get; }

    public IAsyncRelayCommand EditCommand { get; }

    public IAsyncRelayCommand CloseCommand { get; }
}
