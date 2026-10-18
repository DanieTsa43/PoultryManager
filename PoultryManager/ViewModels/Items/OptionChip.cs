using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PoultryManager.ViewModels.Items;

/// <summary>Bouton de filtre (« Toutes », « Bande 1 », « 7 j »…) avec un état sélectionné.</summary>
public sealed partial class OptionChip : ObservableObject
{
    public OptionChip(string label, object? value, Action<OptionChip> onSelected)
    {
        Label = label;
        Value = value;
        SelectCommand = new RelayCommand(() => onSelected(this));
    }

    public string Label { get; }

    /// <summary>Valeur associée (identifiant de bande, nombre de jours…).</summary>
    public object? Value { get; }

    [ObservableProperty]
    private bool isSelected;

    public IRelayCommand SelectCommand { get; }
}
