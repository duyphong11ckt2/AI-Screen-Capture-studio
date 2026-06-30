using AIScreenCaptureStudio.Configuration;

namespace AIScreenCaptureStudio.Services.Interfaces;

public interface IStorageService
{
    AppSettings Settings { get; }
    AppSettings Load();
    void Save();
    void Save(AppSettings settings);
    string AppDataDirectory { get; }
}
