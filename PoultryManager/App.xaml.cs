using Microsoft.Extensions.DependencyInjection;

namespace PoultryManager;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        // InitializeComponent() charge les dictionnaires fusionnés (Colors.xaml, Styles.xaml).
        // Il doit s'exécuter AVANT toute résolution de l'AppShell, dont le XAML référence
        // ces ressources (ex. StaticResource White) : c'est pourquoi l'AppShell n'est plus
        // injecté dans le constructeur mais résolu plus tard, dans CreateWindow.
        InitializeComponent();
        _services = services;

        // L'application suit la maquette (thème clair uniquement).
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = _services.GetRequiredService<AppShell>();
        var window = new Window(shell)
        {
            Title = "PoultryManager"
        };

#if WINDOWS
        // Sur PC, la fenêtre prend le format d'un téléphone (390 × 844 de la maquette).
        window.Width = 410;
        window.Height = 880;
        window.MinimumWidth = 360;
        window.MinimumHeight = 640;
#endif

        return window;
    }
}
