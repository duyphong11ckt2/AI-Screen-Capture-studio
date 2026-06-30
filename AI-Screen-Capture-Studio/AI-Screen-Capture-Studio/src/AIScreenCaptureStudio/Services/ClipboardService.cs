using System.Windows;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.Services;

public sealed class ClipboardService : IClipboardService
{
    public void CopyImage(BitmapSource image)
    {
        RetryStaThread(() => Clipboard.SetImage(image));
    }

    public void CopyText(string text)
    {
        RetryStaThread(() => Clipboard.SetText(text ?? string.Empty));
    }

    private static void RetryStaThread(Action action)
    {
        // The clipboard can be momentarily locked by other apps; retry briefly.
        for (var i = 0; i < 5; i++)
        {
            try { action(); return; }
            catch (System.Runtime.InteropServices.COMException) { System.Threading.Thread.Sleep(40); }
        }
        action();
    }
}
