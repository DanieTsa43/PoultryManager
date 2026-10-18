using System.Diagnostics;
using PoultryManager.ViewModels;

namespace PoultryManager.Views.Base;

/// <summary>
/// Page de base des écrans métier : relie la page à son ViewModel et
/// appelle <see cref="BaseViewModel.OnAppearingAsync"/> à chaque affichage.
/// </summary>
public class ViewModelPage : ContentPage
{
    private readonly BaseViewModel _viewModel;

    protected ViewModelPage(BaseViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.OnAppearingAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PoultryManager] Erreur au chargement de {GetType().Name} : {ex}");
        }
    }
}
