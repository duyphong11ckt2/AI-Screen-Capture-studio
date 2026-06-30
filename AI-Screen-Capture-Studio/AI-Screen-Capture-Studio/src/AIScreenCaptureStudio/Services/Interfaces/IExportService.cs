using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Configuration;

namespace AIScreenCaptureStudio.Services.Interfaces;

public interface IExportService
{
    void SavePng(BitmapSource image, string path);
    void SaveJpg(BitmapSource image, string path, int quality = 92);
    void SavePdf(BitmapSource image, string path);
    void Save(BitmapSource image, string path, ExportFormat format);
}
