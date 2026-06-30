using AIScreenCaptureStudio.Configuration;

namespace AIScreenCaptureStudio.Models;

/// <summary>A single AI run persisted to local history.</summary>
public sealed class HistoryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public AIProviderType Provider { get; set; }
    public string Model { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;

    /// <summary>Raw JSON response from the provider.</summary>
    public string RawResponse { get; set; } = string.Empty;

    /// <summary>Relative file name of the screenshot thumbnail saved alongside.</summary>
    public string? ScreenshotFile { get; set; }
}
