using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace RehabCenterApp.Converters;

public class PriorityToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "High" => new SolidColorBrush(Color.Parse("#e53e3e")),
            "Medium" => new SolidColorBrush(Color.Parse("#ed8936")),
            "Low" => new SolidColorBrush(Color.Parse("#48bb78")),
            _ => new SolidColorBrush(Color.Parse("#718096"))
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}