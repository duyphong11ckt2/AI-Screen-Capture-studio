using System.IO;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Models;
using AIScreenCaptureStudio.Services.Interfaces;
using Tesseract;

namespace AIScreenCaptureStudio.Services;

/// <summary>
/// Local Tesseract OCR. Gracefully reports unavailability when the tessdata
/// directory or language file is missing instead of throwing at startup.
/// </summary>
public sealed class OcrService : IOcrService
{
    private readonly IStorageService _storage;

    public OcrService(IStorageService storage) => _storage = storage;

    public bool IsAvailable
    {
        get
        {
            var dir = _storage.Settings.TessdataPath;
            var lang = _storage.Settings.OcrLanguage;
            return Directory.Exists(dir) && File.Exists(Path.Combine(dir, $"{lang}.traineddata"));
        }
    }

    public Task<OcrResult> ExtractTextAsync(BitmapSource image, CancellationToken ct = default)
        => Task.Run(() =>
        {
            if (!IsAvailable)
            {
                return new OcrResult
                {
                    Text = "OCR unavailable: place '" + _storage.Settings.OcrLanguage +
                           ".traineddata' in the tessdata folder (Settings > OCR).",
                    MeanConfidence = 0
                };
            }

            var pngBytes = ImageHelper.ToPngBytes(image);
            using var engine = new TesseractEngine(_storage.Settings.TessdataPath,
                _storage.Settings.OcrLanguage, EngineMode.Default);
            using var pix = Pix.LoadFromMemory(pngBytes);
            using var page = engine.Process(pix);
            ct.ThrowIfCancellationRequested();
            return new OcrResult
            {
                Text = page.GetText()?.Trim() ?? string.Empty,
                MeanConfidence = page.GetMeanConfidence()
            };
        }, ct);
}
