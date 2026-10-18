namespace PoultryManager.Services.Settings;

/// <summary>Réglages de l'application (écran 12 · Profil et paramètres), conservés sur l'appareil.</summary>
public interface ISettingsService
{
    /// <summary>Poids moyen d'un œuf en grammes (pour l'indice de conversion). 60 g par défaut.</summary>
    double EggWeightGrams { get; set; }

    /// <summary>Rappel de saisie quotidien activé.</summary>
    bool ReminderEnabled { get; set; }

    /// <summary>Heure du rappel (0 à 23). 18 h par défaut.</summary>
    int ReminderHour { get; set; }

    /// <summary>Date de la dernière sauvegarde des données.</summary>
    DateTime? LastBackupDate { get; set; }

    /// <summary>Dernière bande choisie sur le tableau de bord (0 = aucune).</summary>
    int SelectedBandId { get; set; }
}
