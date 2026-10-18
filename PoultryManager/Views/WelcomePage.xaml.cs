using PoultryManager.ViewModels;

namespace PoultryManager.Views;

public partial class WelcomePage : ContentPage
{
    private readonly WelcomeViewModel _viewModel;

    public WelcomePage(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCommand.ExecuteAsync(null);
        await PlayEntranceAnimationAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Stoppe toute animation en cours pour éviter les chevauchements
        // lorsque l'utilisateur quitte puis revient rapidement sur la page.
        CancelEntranceAnimations();
    }

    // Éléments animés, dans leur ordre d'apparition.
    private VisualElement[] AnimatedElements =>
        new VisualElement[] { BackgroundImage, AppNameLabel, TitleLabel, StartButton, LoginButton };

    /// <summary>
    /// Joue l'apparition séquentielle des éléments de la page d'accueil :
    /// image de fond (fondu + léger dézoom), puis nom, titre et boutons en fondu
    /// avec un léger déplacement vertical. L'état final visible est toujours garanti,
    /// même si l'animation est interrompue par une navigation rapide.
    /// </summary>
    private async Task PlayEntranceAnimationAsync()
    {
        ResetEntranceState();

        try
        {
            // 1. Image de fond : révélation progressive (opacité 0 -> 1 + dézoom discret 1.08 -> 1).
            await Task.WhenAll(
                BackgroundImage.FadeToAsync(1, 650, Easing.CubicOut),
                BackgroundImage.ScaleToAsync(1d, 900, Easing.CubicOut));

            // 2. Nom de l'application (le logo de la poule est révélé avec l'image de fond).
            await AnimateAppearAsync(AppNameLabel, 400);

            // 3. Titre principal avec un léger mouvement vers le haut.
            await AnimateAppearAsync(TitleLabel, 400);

            // 4. Bouton « Commencer ».
            await AnimateAppearAsync(StartButton, 350);

            // 5. Bouton « J'ai déjà un compte ».
            await AnimateAppearAsync(LoginButton, 350);
        }
        finally
        {
            // Garantit que tout est visible même si une animation a été interrompue.
            EnsureFinalState();
        }
    }

    private static Task AnimateAppearAsync(VisualElement element, uint duration)
    {
        return Task.WhenAll(
            element.FadeToAsync(1, duration, Easing.CubicOut),
            element.TranslateToAsync(0, 0, duration, Easing.CubicOut));
    }

    private void ResetEntranceState()
    {
        CancelEntranceAnimations();

        BackgroundImage.Opacity = 0;
        BackgroundImage.Scale = 1.08;

        AppNameLabel.Opacity = 0;
        AppNameLabel.TranslationY = 12;

        TitleLabel.Opacity = 0;
        TitleLabel.TranslationY = 18;

        StartButton.Opacity = 0;
        StartButton.TranslationY = 12;

        LoginButton.Opacity = 0;
        LoginButton.TranslationY = 12;
    }

    private void EnsureFinalState()
    {
        BackgroundImage.Opacity = 1;
        BackgroundImage.Scale = 1;

        foreach (var element in new VisualElement[] { AppNameLabel, TitleLabel, StartButton, LoginButton })
        {
            element.Opacity = 1;
            element.TranslationY = 0;
        }
    }

    private void CancelEntranceAnimations()
    {
        foreach (var element in AnimatedElements)
        {
            Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(element);
        }
    }
}
