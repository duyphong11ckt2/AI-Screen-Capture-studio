using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AIScreenCaptureStudio.Converters;

public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) => value is bool b ? !b : value;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => value is bool b ? !b : value;
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public bool Collapse { get; set; } = true;
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var flag = value is bool b && b;
        if (p is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase)) flag = !flag;
        return flag ? Visibility.Visible : (Collapse ? Visibility.Collapsed : Visibility.Hidden);
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => value is Visibility v && v == Visibility.Visible;
}

/// <summary>Maps a 0..1 confidence to a status brush (green/amber/red).</summary>
public sealed class ConfidenceToBrushConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var conf = value is double d ? d : 1.0;
        if (conf >= 0.85) return new SolidColorBrush(Color.FromRgb(0x1D, 0x9E, 0x75));
        if (conf >= 0.6) return new SolidColorBrush(Color.FromRgb(0xBA, 0x75, 0x17));
        return new SolidColorBrush(Color.FromRgb(0xE2, 0x4B, 0x4A));
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class ConfidencePercentConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is double d ? $"{Math.Round(d * 100)}%" : "100%";
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

/// <summary>Enum equality converter for tool-selection toggle buttons.</summary>
public sealed class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value?.ToString() == p?.ToString();
    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => (value is bool b && b && p is not null) ? Enum.Parse(t, p.ToString()!) : Binding.DoNothing;
}
