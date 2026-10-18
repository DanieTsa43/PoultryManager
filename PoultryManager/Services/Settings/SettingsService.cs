using PoultryManager.Constants;

namespace PoultryManager.Services.Settings;

/// <summary>Implémentation de <see cref="ISettingsService"/> basée sur les préférences de l'appareil.</summary>
public sealed class SettingsService : ISettingsService
{
    private const string EggWeightKey = "settings.egg_weight_g";
    private const string ReminderEnabledKey = "settings.reminder_enabled";
    private const string ReminderHourKey = "settings.reminder_hour";
    private const string LastBackupKey = "settings.last_backup_ticks";
    private const string SelectedBandKey = "settings.selected_band_id";

    private readonly IPreferences _preferences;

    public SettingsService(IPreferences preferences)
    {
        _preferences = preferences;
    }

    public double EggWeightGrams
    {
        get => _preferences.Get(EggWeightKey, AppConstants.DefaultEggWeightGrams);
        set => _preferences.Set(EggWeightKey, Math.Clamp(value, 30, 100));
    }

    public bool ReminderEnabled
    {
        get => _preferences.Get(ReminderEnabledKey, true);
        set => _preferences.Set(ReminderEnabledKey, value);
    }

    public int ReminderHour
    {
        get => _preferences.Get(ReminderHourKey, 18);
        set => _preferences.Set(ReminderHourKey, Math.Clamp(value, 0, 23));
    }

    public DateTime? LastBackupDate
    {
        get
        {
            var ticks = _preferences.Get(LastBackupKey, 0L);
            return ticks > 0 ? new DateTime(ticks) : null;
        }
        set => _preferences.Set(LastBackupKey, value?.Ticks ?? 0L);
    }

    public int SelectedBandId
    {
        get => _preferences.Get(SelectedBandKey, 0);
        set => _preferences.Set(SelectedBandKey, value);
    }
}
