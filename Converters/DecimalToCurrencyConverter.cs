using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RehabCenterApp.Converters;

public class DecimalToCurrencyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return d.ToString("C", new CultureInfo("ar-SA"));
        return "0.00 ر.س";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}