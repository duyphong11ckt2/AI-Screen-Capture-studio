using System.Windows.Media.Imaging;

namespace AIScreenCaptureStudio.Models;

public enum CaptureMode
{
    FullScreen,
    Region,
    ActiveWindow
}

/// <summary>Result of a capture operation: the bitmap plus capture metadata.</summary>
public sealed class CaptureResult
{
    public required BitmapSource Image { get; init; }
    public CaptureMode Mode { get; init; }
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    public int PixelWidth => Image.PixelWidth;
    public int PixelHeight => Image.PixelHeight;
}
