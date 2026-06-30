using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AIScreenCaptureStudio.Converters;

/// <summary>
/// Builds an arrow geometry (shaft + head) from a [Width, Height] multi-binding
/// so an arrow annotation can be rendered with a single Path that scales.
/// </summary>
public sealed class ArrowGeometryConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var w = values.Length > 0 && values[0] is double a ? a : 0;
        var h = values.Length > 1 && values[1] is double b ? b : 0;

        var start = new Point(0, 0);
        var end = new Point(Math.Max(1, w), Math.Max(1, h));

        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var len = Math.Max(1, Math.Sqrt(dx * dx + dy * dy));
        var ux = dx / len;
        var uy = dy / len;
        var headLen = Math.Min(22, len * 0.4);
        const double headWidth = 9;

        var baseX = end.X - ux * headLen;
        var baseY = end.Y - uy * headLen;
        var nx = -uy;
        var ny = ux;

        var left = new Point(baseX + nx * headWidth, baseY + ny * headWidth);
        var right = new Point(baseX - nx * headWidth, baseY - ny * headWidth);

        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            // shaft
            ctx.BeginFigure(start, false, false);
            ctx.LineTo(new Point(baseX, baseY), true, true);
            // head
            ctx.BeginFigure(left, true, true);
            ctx.LineTo(end, true, true);
            ctx.LineTo(right, true, true);
        }
        geo.Freeze();
        return geo;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
