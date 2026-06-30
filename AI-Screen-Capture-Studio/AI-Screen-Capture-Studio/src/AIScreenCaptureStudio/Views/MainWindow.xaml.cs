using System.Windows;
using System.Windows.Interop;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.Views;

public partial class MainWindow : Window
{
    private readonly IHotkeyService _hotkeys;
    private readonly IStorageService _storage;

    public MainWindow(IHotkeyService hotkeys, IStorageService storage)
    {
        _hotkeys = hotkeys;
        _storage = storage;
        InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var handle = new WindowInteropHelper(this).Handle;
        _hotkeys.Initialize(handle);
        _hotkeys.Register(_storage.Settings.Hotkeys);

        // Keep only the tray icon; the host window stays hidden.
        Hide();
    }

    public void ShowFromTray()
    {
        // The host window itself is invisible; "show main" surfaces the tray
        // tooltip. Reserved for a future dashboard window.
        Tray.ShowBalloonTip("AI Screen Capture Studio",
            "Running in the system tray. Use the tray menu or hotkeys to capture.",
            Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
    }

    protected override void OnClosed(EventArgs e)
    {
        _hotkeys.Dispose();
        Tray.Dispose();
        base.OnClosed(e);
    }
}
