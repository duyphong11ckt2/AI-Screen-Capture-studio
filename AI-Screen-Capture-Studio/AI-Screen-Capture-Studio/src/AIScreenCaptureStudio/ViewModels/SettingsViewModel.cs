using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Mvvm;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.ViewModels;

public sealed class SettingsViewModel : ObservableObject
{
    private readonly IStorageService _storage;
    private readonly IDialogService _dialog;
    private readonly IHotkeyService _hotkeys;

    public SettingsViewModel(IStorageService storage, IDialogService dialog, IHotkeyService hotkeys,
        AISettingsViewModel aiSettings)
    {
        _storage = storage;
        _dialog = dialog;
        _hotkeys = hotkeys;
        AISettings = aiSettings;

        var s = storage.Settings;
        _saveFolder = s.DefaultSaveFolder;
        _exportFormat = s.DefaultExportFormat;
        _theme = s.Theme;
        _closeToTray = s.CloseToTray;
        _startMinimized = s.StartMinimizedToTray;
        _tessdataPath = s.TessdataPath;
        _ocrLanguage = s.OcrLanguage;

        BrowseFolderCommand = new RelayCommand(_ => BrowseFolder());
        BrowseTessdataCommand = new RelayCommand(_ => BrowseTessdata());
        SaveCommand = new RelayCommand(_ => Save());
    }

    public AISettingsViewModel AISettings { get; }

    private string _saveFolder;
    public string SaveFolder { get => _saveFolder; set => SetProperty(ref _saveFolder, value); }

    private ExportFormat _exportFormat;
    public ExportFormat ExportFormat { get => _exportFormat; set => SetProperty(ref _exportFormat, value); }
    public Array ExportFormats => Enum.GetValues(typeof(ExportFormat));

    private AppTheme _theme;
    public AppTheme Theme { get => _theme; set => SetProperty(ref _theme, value); }
    public Array Themes => Enum.GetValues(typeof(AppTheme));

    private bool _closeToTray;
    public bool CloseToTray { get => _closeToTray; set => SetProperty(ref _closeToTray, value); }

    private bool _startMinimized;
    public bool StartMinimized { get => _startMinimized; set => SetProperty(ref _startMinimized, value); }

    private string _tessdataPath;
    public string TessdataPath { get => _tessdataPath; set => SetProperty(ref _tessdataPath, value); }

    private string _ocrLanguage;
    public string OcrLanguage { get => _ocrLanguage; set => SetProperty(ref _ocrLanguage, value); }

    public HotkeySettings Hotkeys => _storage.Settings.Hotkeys;

    public RelayCommand BrowseFolderCommand { get; }
    public RelayCommand BrowseTessdataCommand { get; }
    public RelayCommand SaveCommand { get; }

    private void BrowseFolder()
    {
        var path = _dialog.ShowFolderBrowserDialog(SaveFolder);
        if (path is not null) SaveFolder = path;
    }

    private void BrowseTessdata()
    {
        var path = _dialog.ShowFolderBrowserDialog(TessdataPath);
        if (path is not null) TessdataPath = path;
    }

    public void Save()
    {
        var s = _storage.Settings;
        s.DefaultSaveFolder = SaveFolder;
        s.DefaultExportFormat = ExportFormat;
        s.Theme = Theme;
        s.CloseToTray = CloseToTray;
        s.StartMinimizedToTray = StartMinimized;
        s.TessdataPath = TessdataPath;
        s.OcrLanguage = OcrLanguage;
        AISettings.Apply();
        _storage.Save();
        _hotkeys.Register(s.Hotkeys);
        _dialog.ShowMessage("Settings saved.");
    }
}
