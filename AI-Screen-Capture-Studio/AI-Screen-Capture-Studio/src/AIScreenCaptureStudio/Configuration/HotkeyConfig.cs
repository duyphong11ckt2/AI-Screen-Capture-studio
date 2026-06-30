namespace AIScreenCaptureStudio.Configuration;

/// <summary>
/// A single configurable global hotkey. Modifiers follow the Win32 MOD_* flags
/// (Alt=1, Control=2, Shift=4, Win=8) combined as a bitmask. Key is a
/// <see cref="System.Windows.Forms.Keys"/> virtual-key value.
/// </summary>
public sealed class HotkeyConfig
{
    public int Modifiers { get; set; }
    public int Key { get; set; }

    public HotkeyConfig() { }

    public HotkeyConfig(int modifiers, int key)
    {
        Modifiers = modifiers;
        Key = key;
    }
}

/// <summary>
/// Holds the three capture hotkeys. Defaults match the product spec.
/// </summary>
public sealed class HotkeySettings
{
    // PrintScreen (VK 0x2C), no modifiers.
    public HotkeyConfig FullScreen { get; set; } = new(0, 0x2C);

    // Ctrl + Shift + S  (Control=2 | Shift=4 = 6), S = 0x53.
    public HotkeyConfig Region { get; set; } = new(6, 0x53);

    // Ctrl + Alt + S  (Alt=1 | Control=2 = 3), S = 0x53.
    public HotkeyConfig ActiveWindow { get; set; } = new(3, 0x53);
}
