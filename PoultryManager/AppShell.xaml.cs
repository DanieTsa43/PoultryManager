using PoultryManager.Constants;
using PoultryManager.Views;

namespace PoultryManager;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    /// <summary>
    /// Enregistre les écrans ouverts « par-dessus » (navigation empilée).
    /// Les écrans racines (Bienvenue et les 5 onglets) sont déclarés dans AppShell.xaml.
    /// </summary>
    private static void RegisterRoutes()
    {
        // Accès
        Routing.RegisterRoute(Routes.Login, typeof(LoginPage));
        Routing.RegisterRoute(Routes.Register, typeof(RegisterPage));

        // Écrans métier
        Routing.RegisterRoute(Routes.BandEdit, typeof(BandEditPage));
        Routing.RegisterRoute(Routes.EggEntry, typeof(EggEntryPage));
        Routing.RegisterRoute(Routes.FeedEntry, typeof(FeedEntryPage));
        Routing.RegisterRoute(Routes.MortalityEntry, typeof(MortalityEntryPage));
        Routing.RegisterRoute(Routes.Indicators, typeof(IndicatorsPage));
        Routing.RegisterRoute(Routes.BandSummary, typeof(BandSummaryPage));
        Routing.RegisterRoute(Routes.ProfileEdit, typeof(ProfileEditPage));
        Routing.RegisterRoute(Routes.ForgotPassword, typeof(ForgotPasswordPage));
        Routing.RegisterRoute(Routes.Farm, typeof(FarmInfoPage));
        Routing.RegisterRoute(Routes.Units, typeof(UnitsPage));
        Routing.RegisterRoute(Routes.Reminders, typeof(RemindersPage));
        Routing.RegisterRoute(Routes.Backup, typeof(BackupPage));
    }
}
