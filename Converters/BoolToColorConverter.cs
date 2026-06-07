using System.Globalization;

namespace ComandaApp.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Retorna un color. Ocupado = Rojo, Libre = Verde.
            return boolValue ? Colors.Red : Colors.Green;
        }

        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // ConvertBack rara vez se usa en este tipo de escenarios (OneWay binding)
        throw new NotImplementedException();
    }
}