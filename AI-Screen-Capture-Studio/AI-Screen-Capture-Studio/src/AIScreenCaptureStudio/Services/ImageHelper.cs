using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AIScreenCaptureStudio.Services;

/// <summary>Conversions between WPF BitmapSource, encoded bytes, and GDI bitmaps.</summary>
public static class ImageHelper
{
    public static byte[] ToPngBytes(BitmapSource image)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }

    public static byte[] ToJpegBytes(BitmapSource image, int quality = 92)
    {
        var encoder = new JpegBitmapEncoder { QualityLevel = quality };
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }

    public static string ToBase64Png(BitmapSource image) => Convert.ToBase64String(ToPngBytes(image));

    public static BitmapSource FromGdiBitmap(System.Drawing.Bitmap bitmap)
    {
        var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        try
        {
            var bmp = BitmapSource.Create(
                bitmap.Width, bitmap.Height, 96, 96,
                PixelFormats.Bgra32, null,
                data.Scan0, data.Stride * bitmap.Height, data.Stride);
            bmp.Freeze();
            return bmp;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    /// <summary>Render a visual (e.g. flattened editor canvas) to a bitmap.</summary>
    public static BitmapSource RenderVisual(Visual visual, int pixelWidth, int pixelHeight, double dpi = 96)
    {
        var rtb = new RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32);
        rtb.Render(visual);
        rtb.Freeze();
        return rtb;
    }

    public static BitmapSource Resize(BitmapSource source, int maxDimension)
    {
        var scale = Math.Min(1.0, (double)maxDimension / Math.Max(source.PixelWidth, source.PixelHeight));
        if (scale >= 1.0) return source;
        var tb = new TransformedBitmap(source, new ScaleTransform(scale, scale));
        tb.Freeze();
        return tb;
    }
}
