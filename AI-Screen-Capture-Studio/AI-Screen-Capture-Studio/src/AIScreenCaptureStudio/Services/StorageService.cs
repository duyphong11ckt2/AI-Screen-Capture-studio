using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.Services;

/// <summary>Loads and persists <see cref="AppSettings"/> as JSON under %APPDATA%.</summary>
public sealed class StorageService : IStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _settingsPath;
    private AppSettings _settings = new();

    public StorageService()
    {
        AppDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AIScreenCaptureStudio");
        Directory.CreateDirectory(AppDataDirectory);
        _settingsPath = Path.Combine(AppDataDirectory, "settings.json");
        Load();
    }

    public string AppDataDirectory { get; }
    public AppSettings Settings => _settings;

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            else
            {
                _settings = new AppSettings();
                Save();
            }
        }
        catch
        {
            _settings = new AppSettings();
        }
        Directory.CreateDirectory(_settings.DefaultSaveFolder);
        return _settings;
    }

    public void Save() => Save(_settings);

    public void Save(AppSettings settings)
    {
        _settings = settings;
        var json = JsonSerializer.Serialize(_settings, JsonOptions);
        File.WriteAllText(_settingsPath, json);
    }
}
