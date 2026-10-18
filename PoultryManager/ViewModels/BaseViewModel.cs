using CommunityToolkit.Mvvm.ComponentModel;

namespace PoultryManager.ViewModels;

/// <summary>
/// Classe de base de tous les ViewModels.
/// [ObservableProperty] génère automatiquement les propriétés notifiantes (CommunityToolkit.Mvvm).
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    /// <summary>Vrai lorsqu'aucun traitement n'est en cours.</summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// Appelé chaque fois que l'écran s'affiche (voir Views/Base/ViewModelPage) :
    /// c'est ici que les écrans rechargent leurs données.
    /// </summary>
    public virtual Task OnAppearingAsync() => Task.CompletedTask;
}
