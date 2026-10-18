namespace PoultryManager.Constants;

/// <summary>
/// Noms des routes de navigation Shell (un par écran de la maquette).
/// Les chemins commençant par « // » réinitialisent la pile de navigation.
/// </summary>
public static class Routes
{
    // ----- Écrans d'accès (hors barre de navigation) -----
    public const string Welcome = "welcome";          // 00 · Bienvenue
    public const string Login = "login";              // 01 · Connexion (empilé)
    public const string Register = "register";        // 02 · Inscription (empilé)

    // ----- Onglets de la barre de navigation (TabBar « main ») -----
    public const string Main = "main";
    public const string Dashboard = "dashboard";      // 03 · Accueil
    public const string Bands = "bands";              // 04 · Bandes
    public const string History = "history";          // 09 · Suivi
    public const string Reports = "reports";          // 11 · Rapports
    public const string Settings = "settings";        // 12 · Plus

    // ----- Écrans empilés depuis les onglets -----
    public const string BandEdit = "bandedit";        // 04b · Nouvelle bande / Modifier
    public const string EggEntry = "eggentry";        // 05 · Production d'œufs
    public const string FeedEntry = "feedentry";      // 06 · Consommation d'aliment
    public const string MortalityEntry = "mortality"; // 07 · Mortalité
    public const string Indicators = "indicators";    // 08 · Indicateurs de rendement
    public const string BandSummary = "bandsummary";  // 10 · Bilan par bande
    public const string ProfileEdit = "profile";      // 12 · Profil utilisateur
    public const string ForgotPassword = "forgotpassword"; // 01 · Mot de passe oublié (depuis Connexion)
    public const string Farm = "farm";                // 12 · Informations de l'exploitation
    public const string Units = "units";              // 12 · Unités de mesure
    public const string Reminders = "reminders";      // 12 · Notifications et rappels
    public const string Backup = "backup";            // 12 · Sauvegarde des données

    // ----- Chemins absolus prêts à l'emploi -----
    public const string WelcomePath = "//" + Welcome;
    public const string DashboardPath = "//" + Main + "/" + Dashboard;
    public const string BandsPath = "//" + Main + "/" + Bands;
    public const string HistoryPath = "//" + Main + "/" + History;
    public const string ReportsPath = "//" + Main + "/" + Reports;
    public const string SettingsPath = "//" + Main + "/" + Settings;

    // ----- Paramètres de navigation -----
    public const string BandIdParameter = "bandId";
    public const string DateParameter = "date";
}
