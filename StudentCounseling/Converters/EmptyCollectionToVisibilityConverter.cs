using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StudentCounseling.Converters;

public class EmptyCollectionToVisibilityConverter : IValueConverter
{
    public bool ShowWhenEmpty { get; set; } = true;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int count = value switch
        {
            ICollection c => c.Count,
            IEnumerable e => CountEnumerable(e),
            _ => 0
        };
        bool empty = count == 0;
        bool show = ShowWhenEmpty ? empty : !empty;
        return show ? Visibility.Visible : Visibility.Collapsed;
    }

    private static int CountEnumerable(IEnumerable e)
    {
        int n = 0;
        foreach (var _ in e) n++;
        return n;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
