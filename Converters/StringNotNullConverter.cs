using System.Globalization;

namespace ComandaApp.Converters;

public class StringNotNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Si el texto no es nulo ni está vacío, devuelve 'true' para hacerlo visible
        return !string.IsNullOrWhiteSpace(value as string);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}