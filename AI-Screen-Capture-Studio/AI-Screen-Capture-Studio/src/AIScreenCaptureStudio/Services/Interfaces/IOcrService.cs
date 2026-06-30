using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Models;

namespace AIScreenCaptureStudio.Services.Interfaces;

public interface IOcrService
{
    Task<OcrResult> ExtractTextAsync(BitmapSource image, CancellationToken ct = default);
    bool IsAvailable { get; }
}
