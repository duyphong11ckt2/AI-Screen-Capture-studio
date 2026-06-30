using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Models;

namespace AIScreenCaptureStudio.Services.Interfaces;

public interface IHotkeyService : IDisposable
{
    event EventHandler<CaptureMode>? HotkeyPressed;
    void Initialize(IntPtr windowHandle);
    void Register(HotkeySettings settings);
    void Unregister();
}
