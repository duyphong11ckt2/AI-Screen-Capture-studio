using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Models;

namespace AIScreenCaptureStudio.Services.Interfaces;

public interface IHistoryService
{
    IReadOnlyList<HistoryEntry> GetAll();
    void Add(HistoryEntry entry, BitmapSource? thumbnail);
    void Delete(Guid id);
    BitmapSource? LoadThumbnail(HistoryEntry entry);
    void Export(HistoryEntry entry, string path);
}
