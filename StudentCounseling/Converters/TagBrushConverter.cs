using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using StudentCounseling.Models;

namespace StudentCounseling.Converters;

public class TagBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var foreground = string.Equals(parameter as string, "Foreground", StringComparison.OrdinalIgnoreCase);
        var key = value switch
        {
            CounselingType type => type.ToString(),
            CounselingMethod method => method.ToString(),
            string s => s,
            _ => string.Empty,
        };

        var (back, text) = key switch
        {
            "개인상담" => ("#E0F2FE", "#075985"),
            "집단상담" => ("#DCFCE7", "#166534"),
            "학부모상담" => ("#FEF3C7", "#92400E"),
            "심리검사" => ("#F3E8FF", "#6B21A8"),
            "대면" => ("#E0E7FF", "#3730A3"),
            "전화" => ("#FFE4E6", "#9F1239"),
            "외부연계" => ("#CCFBF1", "#115E59"),
            "기타" => ("#F3F4F6", "#374151"),
            _ when key.Contains("폭력") || key.Contains("자해") || key.Contains("자살") => ("#FEE2E2", "#991B1B"),
            _ when key.Contains("진로") || key.Contains("학업") || key.Contains("학습") => ("#DBEAFE", "#1E40AF"),
            _ when key.Contains("가정") || key.Contains("가족") || key.Contains("학부모") => ("#FFEDD5", "#9A3412"),
            _ when key.Contains("성격") || key.Contains("대인") => ("#DCFCE7", "#166534"),
            _ when key.Contains("정신") || key.Contains("검사") => ("#F3E8FF", "#6B21A8"),
            _ => ("#F3F4F6", "#374151"),
        };

        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(foreground ? text : back));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
