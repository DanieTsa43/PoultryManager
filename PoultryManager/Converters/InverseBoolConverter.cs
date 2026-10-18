using System.Globalization;

namespace PoultryManager.Converters;

/// <summary>
/// Inverse une valeur booléenne (true → false, false → true).
/// Exemple : IsVisible="{Binding IsBusy, Converter={StaticResource InverseBoolConverter}}".
/// </summary>
public sealed class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : true;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}
