using System.Windows;

namespace AIScreenCaptureStudio.Native;

/// <summary>
/// Helpers for working with the (possibly multi-monitor, high-DPI) virtual
/// desktop. Coordinates are returned in device pixels.
/// </summary>
public static class ScreenHelper
{
    public sealed record VirtualBounds(int Left, int Top, int Width, int Height);

    /// <summary>The bounding rectangle of all monitors combined, in device pixels.</summary>
    public static VirtualBounds GetVirtualScreen()
    {
        var b = System.Windows.Forms.SystemInformation.VirtualScreen;
        return new VirtualBounds(b.Left, b.Top, b.Width, b.Height);
    }

    /// <summary>All monitor working rectangles in device pixels.</summary>
    public static IReadOnlyList<VirtualBounds> GetAllScreens()
    {
        var list = new List<VirtualBounds>();
        foreach (var s in System.Windows.Forms.Screen.AllScreens)
            list.Add(new VirtualBounds(s.Bounds.Left, s.Bounds.Top, s.Bounds.Width, s.Bounds.Height));
        return list;
    }

    /// <summary>
    /// Returns the DPI scale (1.0 == 96 DPI) for the monitor that contains the
    /// supplied WPF visual, falling back to the primary monitor scale.
    /// </summary>
    public static double GetDpiScale(Visual? visual)
    {
        if (visual is not null)
        {
            var source = PresentationSource.FromVisual(visual);
            if (source?.CompositionTarget is not null)
                return source.CompositionTarget.TransformToDevice.M11;
        }
        return 1.0;
    }
}
