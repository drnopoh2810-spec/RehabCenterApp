using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RehabCenterApp.Converters;

public class DateToRelativeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
        {
            var diff = DateTime.Now - dt;
            if (diff.TotalMinutes < 1) return "الآن";
            if (diff.TotalHours < 1) return $"{diff.Minutes} دقيقة";
            if (diff.TotalDays < 1) return $"{diff.Hours} ساعة";
            if (diff.TotalDays < 30) return $"{diff.Days} يوم";
            return dt.ToString("yyyy-MM-dd");
        }
        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}