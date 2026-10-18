namespace PoultryManager.Constants;

/// <summary>Valeurs fixes de l'application.</summary>
public static class AppConstants
{
    /// <summary>Nom du fichier de base de données SQLite (dans le dossier de données de l'application).</summary>
    public const string DatabaseFileName = "poultrymanager.db3";

    /// <summary>Poids moyen d'un œuf par défaut, en grammes (réglable dans « Unités de mesure »).</summary>
    public const double DefaultEggWeightGrams = 60;

    /// <summary>Longueur minimale d'un mot de passe.</summary>
    public const int MinPasswordLength = 6;

    public static readonly IReadOnlyList<string> Strains =
        ["Souche légère", "Souche mi-lourde", "Non renseignée"];

    public static readonly IReadOnlyList<string> FeedTypes =
        ["Aliment pondeuse", "Aliment pré-ponte", "Autre"];

    public static readonly IReadOnlyList<string> EggCategories =
        ["Moyen", "Petit", "Gros", "Non renseignée"];

    public static readonly IReadOnlyList<string> MortalityCauses =
        ["Inconnue", "Maladie", "Accident", "Chaleur", "Prédation"];
}
