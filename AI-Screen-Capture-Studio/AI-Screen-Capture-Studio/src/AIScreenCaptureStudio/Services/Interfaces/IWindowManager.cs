using AIScreenCaptureStudio.Models;

namespace AIScreenCaptureStudio.Services.Interfaces;

/// <summary>Creates and shows top-level windows via DI (keeps VMs view-agnostic).</summary>
public interface IWindowManager
{
    void ShowEditor(CaptureResult capture);
    void ShowSettings();
    void ShowHistory();
    void ShowMain();
    System.Windows.Int32Rect? ShowRegionSelector();
}
