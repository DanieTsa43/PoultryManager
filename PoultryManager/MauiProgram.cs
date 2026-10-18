using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using PoultryManager.Constants;
using PoultryManager.Data;
using PoultryManager.Data.Repositories;
using PoultryManager.Services.Auth;
using PoultryManager.Services.Dialogs;
using PoultryManager.Services.Farm;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Platform;
using PoultryManager.Services.Session;
using PoultryManager.Services.Settings;
using PoultryManager.Services.Sharing;
using PoultryManager.Services.Startup;
using PoultryManager.ViewModels;
using PoultryManager.Views;

namespace PoultryManager;

public static class MauiProgram
{
    // Clé sentinelle : marque que les ressources de thème des Entry (WinUI) ont déjà été
    // surchargées, pour ne le faire qu'une seule fois au niveau de l'application.
    private const string EntryThemeOverrideFlag = "PoultryManagerEntryThemeOverridden";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("PlusJakartaSans-Regular.ttf", "JakartaRegular");
                fonts.AddFont("PlusJakartaSans-Medium.ttf", "JakartaMedium");
                fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "JakartaSemiBold");
                fonts.AddFont("PlusJakartaSans-Bold.ttf", "JakartaBold");
                fonts.AddFont("PlusJakartaSans-ExtraBold.ttf", "JakartaExtraBold");
            });

        ConfigureEntryAppearance();
        ConfigureBottomTabBar();

        builder.Services
            .AddAppServices()
            .AddViewModels()
            .AddViews();

#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

        return builder.Build();
    }

    /// <summary>Services (navigation, données, authentification…).</summary>
    private static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddSingleton<AppShell>();

        // Base de données locale SQLite et dépôts
        services.AddSingleton(new AppDatabase(
            Path.Combine(FileSystem.AppDataDirectory, AppConstants.DatabaseFileName)));
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IBandRepository, BandRepository>();
        services.AddSingleton<IRecordRepository, RecordRepository>();
        services.AddSingleton<IAppInitializer, AppInitializer>();

        // Services métier
        services.AddSingleton<IFarmService, FarmService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<AppState>();
        services.AddSingleton<IFileShareService, FileShareService>();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IPreferences>(Preferences.Default);
        return services;
    }

    /// <summary>ViewModels : un par écran.</summary>
    private static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        // Accès
        services.AddTransient<WelcomeViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();

        // Onglets
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<BandsViewModel>();
        services.AddTransient<HistoryViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Écrans empilés
        services.AddTransient<BandEditViewModel>();
        services.AddTransient<EggEntryViewModel>();
        services.AddTransient<FeedEntryViewModel>();
        services.AddTransient<MortalityEntryViewModel>();
        services.AddTransient<IndicatorsViewModel>();
        services.AddTransient<BandSummaryViewModel>();
        services.AddTransient<ProfileEditViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();
        services.AddTransient<FarmInfoViewModel>();
        services.AddTransient<UnitsViewModel>();
        services.AddTransient<RemindersViewModel>();
        services.AddTransient<BackupViewModel>();
        return services;
    }

    /// <summary>Vues (pages XAML).</summary>
    private static IServiceCollection AddViews(this IServiceCollection services)
    {
        // Accès
        services.AddTransient<WelcomePage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<RegisterPage>();

        // Onglets
        services.AddTransient<DashboardPage>();
        services.AddTransient<BandsPage>();
        services.AddTransient<HistoryPage>();
        services.AddTransient<ReportsPage>();
        services.AddTransient<SettingsPage>();

        // Écrans empilés
        services.AddTransient<BandEditPage>();
        services.AddTransient<EggEntryPage>();
        services.AddTransient<FeedEntryPage>();
        services.AddTransient<MortalityEntryPage>();
        services.AddTransient<IndicatorsPage>();
        services.AddTransient<BandSummaryPage>();
        services.AddTransient<ProfileEditPage>();
        services.AddTransient<ForgotPasswordPage>();
        services.AddTransient<FarmInfoPage>();
        services.AddTransient<UnitsPage>();
        services.AddTransient<RemindersPage>();
        services.AddTransient<BackupPage>();
        return services;
    }

    /// <summary>
    /// Retire le soulignement / la bordure natifs des champs de saisie :
    /// le contour est dessiné par un Border XAML, conformément à la maquette.
    /// </summary>
    private static void ConfigureEntryAppearance()
    {
        EntryHandler.Mapper.AppendToMapping("PoultryManagerNoNativeBorder", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                global::Android.Content.Res.ColorStateList.ValueOf(global::Android.Graphics.Color.Transparent);
#elif WINDOWS
            // Les propriétés simples (fond, épaisseur de bordure) se posent sans risque
            // directement sur le contrôle.
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

            // En revanche, modifier le ResourceDictionary d'un contrôle WinUI DÉJÀ connecté à
            // l'arbre visuel (ce qui est le cas ici, dans le mapper du handler) lève une
            // COMException (E_NOINTERFACE) — c'est ce qui faisait planter l'ouverture des pages
            // contenant des Entry (ex. Inscription, au clic sur « Commencer »).
            // On surcharge donc ces clés de thème une seule fois, au niveau des ressources de
            // l'application WinUI, ce qui s'applique à tous les Entry sans toucher au contrôle.
            var appResources = Microsoft.UI.Xaml.Application.Current?.Resources;
            if (appResources is not null && !appResources.ContainsKey(EntryThemeOverrideFlag))
            {
                appResources[EntryThemeOverrideFlag] = true;
                appResources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
                appResources["TextControlBackgroundFocused"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                appResources["TextControlBackgroundPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }
#endif
        });

        // Même traitement pour les listes déroulantes, les dates et les zones de texte.
        PickerHandler.Mapper.AppendToMapping("PoultryManagerNoNativeBorder", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                global::Android.Content.Res.ColorStateList.ValueOf(global::Android.Graphics.Color.Transparent);
#elif WINDOWS
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
#endif
        });

        DatePickerHandler.Mapper.AppendToMapping("PoultryManagerNoNativeBorder", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                global::Android.Content.Res.ColorStateList.ValueOf(global::Android.Graphics.Color.Transparent);
#elif WINDOWS
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
#endif
        });

        EditorHandler.Mapper.AppendToMapping("PoultryManagerNoNativeBorder", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                global::Android.Content.Res.ColorStateList.ValueOf(global::Android.Graphics.Color.Transparent);
#elif WINDOWS
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
#endif
        });
    }

    /// <summary>
    /// Sous Windows (WinUI), la barre d'onglets du Shell (Accueil, Bandes, Suivi, Rapports, Plus)
    /// s'affiche par défaut EN HAUT de la fenêtre : il s'agit d'une limitation connue de .NET MAUI
    /// (cf. dotnet/maui#6369), que l'on ne peut pas corriger en pur XAML.
    /// Pour respecter la maquette (barre en bas, comme sur mobile), on repositionne la zone des
    /// onglets (« TopNavArea » du NavigationView WinUI) tout en bas, une fois son gabarit appliqué.
    /// Android affiche déjà les onglets en bas : aucun traitement n'est nécessaire.
    /// </summary>
    private static void ConfigureBottomTabBar()
    {
#if WINDOWS
        Microsoft.Maui.Controls.Handlers.ShellItemHandler.Mapper.AppendToMapping(
            "PoultryManagerBottomTabs", (handler, item) =>
            {
                if (handler.PlatformView is not Microsoft.UI.Xaml.Controls.NavigationView navView)
                {
                    return;
                }

                void Reposition()
                {
                    try
                    {
                        MoveTabStripToBottom(navView);
                    }
                    catch
                    {
                        // Simple amélioration visuelle : en cas d'échec (changement de gabarit WinUI
                        // dans une future version), on conserve la barre en haut plutôt que de
                        // risquer un plantage au démarrage.
                    }
                }

                if (navView.IsLoaded)
                {
                    Reposition();
                }
                else
                {
                    // Le déplacement est idempotent (cf. MoveTabStripToBottom), donc même si
                    // l'événement se déclenche plusieurs fois, la barre n'est déplacée qu'une fois.
                    navView.Loaded += (_, _) => Reposition();
                }
            });
#endif
    }

#if WINDOWS
    /// <summary>
    /// Déplace la zone des onglets (« TopNavArea ») du NavigationView WinUI tout en bas de sa grille
    /// racine en lui ajoutant une rangée dédiée. L'opération est idempotente (marquage via Tag).
    /// </summary>
    private static void MoveTabStripToBottom(Microsoft.UI.Xaml.Controls.NavigationView navView)
    {
        if (FindDescendantByName(navView, "TopNavArea") is not Microsoft.UI.Xaml.FrameworkElement topNavArea)
        {
            return;
        }

        // Ne déplacer qu'une seule fois : le mapper et l'événement Loaded peuvent se répéter.
        const string MovedMarker = "PoultryManagerTabsMovedToBottom";
        if (topNavArea.Tag is string marker && marker == MovedMarker)
        {
            return;
        }

        if (Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(topNavArea)
                is not Microsoft.UI.Xaml.Controls.Grid rootGrid)
        {
            return;
        }

        // On ajoute une rangée auto tout en bas et on y place la barre d'onglets. La rangée
        // d'origine (Auto) se retrouve vide et se réduit à 0 : le contenu occupe alors tout
        // l'espace, et la barre se colle en bas de la fenêtre.
        rootGrid.RowDefinitions.Add(new Microsoft.UI.Xaml.Controls.RowDefinition
        {
            Height = Microsoft.UI.Xaml.GridLength.Auto
        });
        int bottomRow = rootGrid.RowDefinitions.Count - 1;
        Microsoft.UI.Xaml.Controls.Grid.SetRow(topNavArea, bottomRow);
        topNavArea.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Bottom;
        topNavArea.Tag = MovedMarker;
    }

    /// <summary>Recherche récursive d'un élément de l'arbre visuel par son nom.</summary>
    private static Microsoft.UI.Xaml.FrameworkElement? FindDescendantByName(
        Microsoft.UI.Xaml.DependencyObject root, string name)
    {
        int count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(root, i);
            if (child is Microsoft.UI.Xaml.FrameworkElement fe && fe.Name == name)
            {
                return fe;
            }

            if (FindDescendantByName(child, name) is { } found)
            {
                return found;
            }
        }

        return null;
    }
#endif
}
