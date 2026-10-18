// Pour en savoir plus sur WinUI et la structure du projet, voir : https://aka.ms/winui-project-info

namespace PoultryManager.WinUI;

/// <summary>
/// Point d'entrée Windows de l'application.
/// </summary>
public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
