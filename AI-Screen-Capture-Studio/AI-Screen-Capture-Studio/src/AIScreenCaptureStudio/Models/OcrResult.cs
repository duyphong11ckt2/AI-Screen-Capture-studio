namespace AIScreenCaptureStudio.Models;

public sealed class OcrResult
{
    public string Text { get; set; } = string.Empty;
    public float MeanConfidence { get; set; }
}
