using System.Windows;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Models;

namespace AIScreenCaptureStudio.Services.Interfaces;

public interface ICaptureService
{
    CaptureResult CaptureFullScreen();
    CaptureResult CaptureActiveWindow();
    /// <summary>Capture an arbitrary device-pixel rectangle of the virtual desktop.</summary>
    CaptureResult CaptureRegion(Int32Rect deviceRect);
    BitmapSource CaptureRegionBitmap(Int32Rect deviceRect);
}
