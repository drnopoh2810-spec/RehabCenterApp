using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace RehabCenterApp.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Active" or "Present" or "Paid" or "Completed" => new SolidColorBrush(Color.Parse("#48bb78")),
            "Inactive" or "Absent" or "Outstanding" => new SolidColorBrush(Color.Parse("#e53e3e")),
            "Scheduled" or "Pending" => new SolidColorBrush(Color.Parse("#ed8936")),
            "Cancelled" => new SolidColorBrush(Color.Parse("#718096")),
            _ => new SolidColorBrush(Color.Parse("#718096"))
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}