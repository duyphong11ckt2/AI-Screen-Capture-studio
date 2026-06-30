using System.Windows.Interop;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Models;
using AIScreenCaptureStudio.Native;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.Services;

/// <summary>
/// Registers system-wide hotkeys against a host window and raises
/// <see cref="HotkeyPressed"/> on the UI thread when one fires.
/// </summary>
public sealed class HotkeyService : IHotkeyService
{
    private const int IdFullScreen = 9001;
    private const int IdRegion = 9002;
    private const int IdActiveWindow = 9003;

    private IntPtr _handle;
    private HwndSource? _source;
    private bool _registered;

    public event EventHandler<CaptureMode>? HotkeyPressed;

    public void Initialize(IntPtr windowHandle)
    {
        _handle = windowHandle;
        _source = HwndSource.FromHwnd(_handle);
        _source?.AddHook(WndProc);
    }

    public void Register(HotkeySettings settings)
    {
        if (_handle == IntPtr.Zero) return;
        Unregister();

        TryRegister(IdFullScreen, settings.FullScreen);
        TryRegister(IdRegion, settings.Region);
        TryRegister(IdActiveWindow, settings.ActiveWindow);
        _registered = true;
    }

    private void TryRegister(int id, HotkeyConfig cfg)
    {
        if (cfg.Key == 0) return;
        var mods = (uint)cfg.Modifiers | (uint)NativeMethods.HotkeyModifiers.NoRepeat;
        NativeMethods.RegisterHotKey(_handle, id, mods, (uint)cfg.Key);
    }

    public void Unregister()
    {
        if (_handle == IntPtr.Zero || !_registered) return;
        NativeMethods.UnregisterHotKey(_handle, IdFullScreen);
        NativeMethods.UnregisterHotKey(_handle, IdRegion);
        NativeMethods.UnregisterHotKey(_handle, IdActiveWindow);
        _registered = false;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            var id = wParam.ToInt32();
            CaptureMode? mode = id switch
            {
                IdFullScreen => CaptureMode.FullScreen,
                IdRegion => CaptureMode.Region,
                IdActiveWindow => CaptureMode.ActiveWindow,
                _ => null
            };
            if (mode is not null)
            {
                HotkeyPressed?.Invoke(this, mode.Value);
                handled = true;
            }
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
        _source?.RemoveHook(WndProc);
    }
}
