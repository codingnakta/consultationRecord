using System;
using System.Globalization;
using System.Windows.Data;

namespace StudentCounseling.Converters;

public class StringTruncateConverter : IValueConverter
{
    public int MaxLength { get; set; } = 40;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var s = value as string ?? string.Empty;
        s = s.Replace("\r", " ").Replace("\n", " ");
        if (s.Length <= MaxLength) return s;
        return s.Substring(0, MaxLength) + "…";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
