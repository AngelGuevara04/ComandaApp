using System.Globalization;

namespace ComandaApp.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Validamos si el valor entrante es un booleano usando camelCase
        if (value is bool estaOcupada)
        {
            // Rojo para ocupada, verde para libre
            return estaOcupada ? Colors.Red : Colors.Green;
        }

        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // ConvertBack rara vez se usa en este tipo de escenarios (OneWay binding)
        throw new NotImplementedException();
    }
}