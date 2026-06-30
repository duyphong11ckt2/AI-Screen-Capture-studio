using System.IO;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Services.Interfaces;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace AIScreenCaptureStudio.Services;

/// <summary>Writes a flattened bitmap to PNG, JPG, or single-page PDF.</summary>
public sealed class ExportService : IExportService
{
    public void Save(BitmapSource image, string path, ExportFormat format)
    {
        switch (format)
        {
            case ExportFormat.Png: SavePng(image, path); break;
            case ExportFormat.Jpg: SaveJpg(image, path); break;
            case ExportFormat.Pdf: SavePdf(image, path); break;
        }
    }

    public void SavePng(BitmapSource image, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var fs = File.Create(path);
        encoder.Save(fs);
    }

    public void SaveJpg(BitmapSource image, string path, int quality = 92)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var encoder = new JpegBitmapEncoder { QualityLevel = quality };
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var fs = File.Create(path);
        encoder.Save(fs);
    }

    public void SavePdf(BitmapSource image, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var pngBytes = ImageHelper.ToPngBytes(image);

        using var doc = new PdfDocument();
        var page = doc.AddPage();
        page.Width = XUnit.FromPoint(image.PixelWidth * 72.0 / 96.0);
        page.Height = XUnit.FromPoint(image.PixelHeight * 72.0 / 96.0);

        using var gfx = XGraphics.FromPdfPage(page);
        using var ms = new MemoryStream(pngBytes);
        using var ximg = XImage.FromStream(ms);
        gfx.DrawImage(ximg, 0, 0, page.Width.Point, page.Height.Point);

        doc.Save(path);
    }
}
