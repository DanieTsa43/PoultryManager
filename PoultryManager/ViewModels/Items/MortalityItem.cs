using CommunityToolkit.Mvvm.Input;
using PoultryManager.Helpers;
using PoultryManager.Models;

namespace PoultryManager.ViewModels.Items;

/// <summary>Ligne « Dernières saisies » de l'écran 07 · Mortalité.</summary>
public sealed class MortalityItem
{
    public MortalityItem(MortalityRecord record, Func<MortalityItem, Task> edit, Func<MortalityItem, Task> delete)
    {
        Record = record;
        EditCommand = new AsyncRelayCommand(() => edit(this));
        DeleteCommand = new AsyncRelayCommand(() => delete(this));
    }

    public MortalityRecord Record { get; }

    /// <summary>« 17/05/2025 · 1 mort ».</summary>
    public string Title => $"{Formats.ShortDate(Record.Date)} · {Formats.Plural(Record.Deaths, "mort", "morts")}";

    public string Subtitle => string.IsNullOrWhiteSpace(Record.Cause) ? "Cause non renseignée" : $"Cause : {Record.Cause.ToLowerInvariant()}";

    public string EditDescription => $"Corriger la saisie du {Formats.ShortDate(Record.Date)}";

    public string DeleteDescription => $"Supprimer la saisie du {Formats.ShortDate(Record.Date)}";

    public IAsyncRelayCommand EditCommand { get; }

    public IAsyncRelayCommand DeleteCommand { get; }
}
