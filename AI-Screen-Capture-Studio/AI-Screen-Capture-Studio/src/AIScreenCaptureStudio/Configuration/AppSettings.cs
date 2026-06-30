using System.IO;

namespace AIScreenCaptureStudio.Configuration;

/// <summary>
/// Root settings object persisted as JSON under %APPDATA%.
/// </summary>
public sealed class AppSettings
{
    public string DefaultSaveFolder { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "AI Screen Capture Studio");

    public ExportFormat DefaultExportFormat { get; set; } = ExportFormat.Png;

    public AppTheme Theme { get; set; } = AppTheme.Dark;

    public HotkeySettings Hotkeys { get; set; } = new();

    public AIProviderConfig AI { get; set; } = new();

    public PromptTemplates Prompts { get; set; } = new();

    public bool StartMinimizedToTray { get; set; } = true;

    public bool CloseToTray { get; set; } = true;

    public string TessdataPath { get; set; } =
        Path.Combine(AppContext.BaseDirectory, "tessdata");

    public string OcrLanguage { get; set; } = "eng";
}
