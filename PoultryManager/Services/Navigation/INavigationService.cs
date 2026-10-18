namespace PoultryManager.Services.Navigation;

/// <summary>
/// Navigation entre les écrans, utilisée par les ViewModels
/// (aucun ViewModel ne manipule directement les pages).
/// </summary>
public interface INavigationService
{
    /// <summary>Ouvre l'écran correspondant à la route.</summary>
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);

    /// <summary>Revient à l'écran précédent.</summary>
    Task GoBackAsync();
}
