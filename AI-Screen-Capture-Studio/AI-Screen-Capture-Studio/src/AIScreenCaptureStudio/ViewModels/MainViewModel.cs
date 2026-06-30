using System.Windows;
using AIScreenCaptureStudio.Models;
using AIScreenCaptureStudio.Mvvm;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.ViewModels;

/// <summary>
/// Drives the tray menu and main window. Owns the capture-to-editor flow and
/// reacts to global hotkeys.
/// </summary>
public sealed class MainViewModel : ObservableObject
{
    private readonly ICaptureService _capture;
    private readonly IWindowManager _windows;
    private readonly IHotkeyService _hotkeys;
    private readonly IStorageService _storage;

    public MainViewModel(ICaptureService capture, IWindowManager windows,
        IHotkeyService hotkeys, IStorageService storage)
    {
        _capture = capture;
        _windows = windows;
        _hotkeys = hotkeys;
        _storage = storage;

        _hotkeys.HotkeyPressed += (_, mode) => Application.Current.Dispatcher.Invoke(() => Capture(mode));

        CaptureFullScreenCommand = new RelayCommand(_ => Capture(CaptureMode.FullScreen));
        CaptureRegionCommand = new RelayCommand(_ => Capture(CaptureMode.Region));
        CaptureActiveWindowCommand = new RelayCommand(_ => Capture(CaptureMode.ActiveWindow));
        OpenSettingsCommand = new RelayCommand(_ => _windows.ShowSettings());
        OpenHistoryCommand = new RelayCommand(_ => _windows.ShowHistory());
        ShowMainCommand = new RelayCommand(_ => _windows.ShowMain());
        ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
    }

    public string AppTitle => "AI Screen Capture Studio";
    public string SaveFolder => _storage.Settings.DefaultSaveFolder;

    public RelayCommand CaptureFullScreenCommand { get; }
    public RelayCommand CaptureRegionCommand { get; }
    public RelayCommand CaptureActiveWindowCommand { get; }
    public RelayCommand OpenSettingsCommand { get; }
    public RelayCommand OpenHistoryCommand { get; }
    public RelayCommand ShowMainCommand { get; }
    public RelayCommand ExitCommand { get; }

    public void Capture(CaptureMode mode)
    {
        CaptureResult? result = mode switch
        {
            CaptureMode.FullScreen => _capture.CaptureFullScreen(),
            CaptureMode.ActiveWindow => _capture.CaptureActiveWindow(),
            CaptureMode.Region => CaptureRegion(),
            _ => null
        };
        if (result is not null) _windows.ShowEditor(result);
    }

    private CaptureResult? CaptureRegion()
    {
        var rect = _windows.ShowRegionSelector();
        if (rect is null || rect.Value.Width <= 0 || rect.Value.Height <= 0) return null;
        return _capture.CaptureRegion(rect.Value);
    }
}
