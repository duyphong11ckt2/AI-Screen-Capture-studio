using System.Windows;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Models;
using AIScreenCaptureStudio.Native;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.Services;

/// <summary>
/// GDI-based screen capture covering the full virtual desktop, an arbitrary
/// region, or the active foreground window (using its DWM frame bounds so the
/// drop shadow is excluded).
/// </summary>
public sealed class CaptureService : ICaptureService
{
    public CaptureResult CaptureFullScreen()
    {
        var v = ScreenHelper.GetVirtualScreen();
        var rect = new Int32Rect(v.Left, v.Top, v.Width, v.Height);
        return new CaptureResult { Image = CaptureRegionBitmap(rect), Mode = CaptureMode.FullScreen };
    }

    public CaptureResult CaptureRegion(Int32Rect deviceRect)
        => new() { Image = CaptureRegionBitmap(deviceRect), Mode = CaptureMode.Region };

    public CaptureResult CaptureActiveWindow()
    {
        var hwnd = NativeMethods.GetForegroundWindow();
        NativeMethods.RECT rect;

        if (hwnd == IntPtr.Zero ||
            NativeMethods.DwmGetWindowAttribute(hwnd, NativeMethods.DWMWA_EXTENDED_FRAME_BOUNDS,
                out rect, System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.RECT>()) != 0)
        {
            if (hwnd == IntPtr.Zero || !NativeMethods.GetWindowRect(hwnd, out rect))
                return CaptureFullScreen();
        }

        var area = new Int32Rect(rect.Left, rect.Top, Math.Max(1, rect.Width), Math.Max(1, rect.Height));
        return new CaptureResult { Image = CaptureRegionBitmap(area), Mode = CaptureMode.ActiveWindow };
    }

    public BitmapSource CaptureRegionBitmap(Int32Rect deviceRect)
    {
        var width = Math.Max(1, deviceRect.Width);
        var height = Math.Max(1, deviceRect.Height);

        using var bmp = new System.Drawing.Bitmap(width, height,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = System.Drawing.Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(deviceRect.X, deviceRect.Y, 0, 0,
                new System.Drawing.Size(width, height),
                System.Drawing.CopyPixelOperation.SourceCopy);
        }
        return ImageHelper.FromGdiBitmap(bmp);
    }
}
