using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Models;

namespace AIScreenCaptureStudio.Providers;

/// <summary>
/// Abstraction over a vision-capable LLM provider. New providers can be added
/// without touching the UI by implementing this interface and registering with
/// the factory.
/// </summary>
public interface IAIProvider
{
    AIProviderType ProviderType { get; }
    string DisplayName { get; }

    Task<ConnectionTestResult> TestConnectionAsync(AIProviderRequestContext ctx, CancellationToken ct = default);

    /// <summary>Low-level call: send image + prompt, return raw JSON text.</summary>
    Task<string> AnalyzeImageAsync(BitmapSource image, string prompt, AIProviderRequestContext ctx, CancellationToken ct = default);

    Task<AIAnalysisResult> GenerateGuideAsync(BitmapSource image, string prompt, AIProviderRequestContext ctx, CancellationToken ct = default);

    Task<AIAnalysisResult> GenerateAnnotationsAsync(BitmapSource image, string prompt, AIProviderRequestContext ctx, CancellationToken ct = default);
}

/// <summary>Per-request connection settings (endpoint, model, decrypted key).</summary>
public sealed class AIProviderRequestContext
{
    public required string Endpoint { get; init; }
    public required string Model { get; init; }
    public required string ApiKey { get; init; }
}
