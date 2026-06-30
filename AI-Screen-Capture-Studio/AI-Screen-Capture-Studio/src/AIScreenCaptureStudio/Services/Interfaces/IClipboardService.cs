using System.Windows.Media.Imaging;

namespace AIScreenCaptureStudio.Services.Interfaces;

public interface IClipboardService
{
    void CopyImage(BitmapSource image);
    void CopyText(string text);
}
