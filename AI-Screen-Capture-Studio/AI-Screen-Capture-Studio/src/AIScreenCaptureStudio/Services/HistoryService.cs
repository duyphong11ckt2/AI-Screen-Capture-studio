using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Models;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.Services;

/// <summary>Stores AI run history (metadata + thumbnail) locally as JSON + PNGs.</summary>
public sealed class HistoryService : IHistoryService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _historyDir;
    private readonly string _thumbsDir;
    private readonly string _indexPath;
    private readonly List<HistoryEntry> _entries = new();

    public HistoryService(IStorageService storage)
    {
        _historyDir = Path.Combine(storage.AppDataDirectory, "history");
        _thumbsDir = Path.Combine(_historyDir, "thumbnails");
        Directory.CreateDirectory(_thumbsDir);
        _indexPath = Path.Combine(_historyDir, "history.json");
        LoadIndex();
    }

    private void LoadIndex()
    {
        try
        {
            if (File.Exists(_indexPath))
            {
                var list = JsonSerializer.Deserialize<List<HistoryEntry>>(File.ReadAllText(_indexPath), JsonOptions);
                if (list is not null) _entries.AddRange(list);
            }
        }
        catch { /* corrupt index: start fresh */ }
    }

    private void SaveIndex() =>
        File.WriteAllText(_indexPath, JsonSerializer.Serialize(_entries, JsonOptions));

    public IReadOnlyList<HistoryEntry> GetAll() =>
        _entries.OrderByDescending(e => e.TimestampUtc).ToList();

    public void Add(HistoryEntry entry, BitmapSource? thumbnail)
    {
        if (thumbnail is not null)
        {
            var thumb = ImageHelper.Resize(thumbnail, 480);
            var file = $"{entry.Id}.png";
            using var fs = File.Create(Path.Combine(_thumbsDir, file));
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(thumb));
            enc.Save(fs);
            entry.ScreenshotFile = file;
        }
        _entries.Add(entry);
        SaveIndex();
    }

    public void Delete(Guid id)
    {
        var entry = _entries.FirstOrDefault(e => e.Id == id);
        if (entry is null) return;
        if (entry.ScreenshotFile is not null)
        {
            var p = Path.Combine(_thumbsDir, entry.ScreenshotFile);
            if (File.Exists(p)) File.Delete(p);
        }
        _entries.Remove(entry);
        SaveIndex();
    }

    public BitmapSource? LoadThumbnail(HistoryEntry entry)
    {
        if (entry.ScreenshotFile is null) return null;
        var path = Path.Combine(_thumbsDir, entry.ScreenshotFile);
        if (!File.Exists(path)) return null;
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.UriSource = new Uri(path);
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    public void Export(HistoryEntry entry, string path) =>
        File.WriteAllText(path, JsonSerializer.Serialize(entry, JsonOptions));
}
