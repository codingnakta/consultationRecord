using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StudentCounseling.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isNull = value switch
        {
            null => true,
            string s => string.IsNullOrWhiteSpace(s),
            bool b => !b,
            _ => false,
        };
        bool visible = Invert ? isNull : !isNull;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
