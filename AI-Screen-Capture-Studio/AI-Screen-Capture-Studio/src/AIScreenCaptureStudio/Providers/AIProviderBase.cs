using System.Text.Json;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Models;
using AIScreenCaptureStudio.Services;

namespace AIScreenCaptureStudio.Providers;

/// <summary>
/// Shared plumbing for HTTP providers: image encoding, JSON extraction from
/// (possibly fenced) model output, and deserialization to AIAnalysisResult.
/// </summary>
public abstract class AIProviderBase : IAIProvider
{
    protected readonly HttpClient Http;

    protected AIProviderBase(HttpClient http) => Http = http;

    public abstract AIProviderType ProviderType { get; }
    public abstract string DisplayName { get; }

    public abstract Task<ConnectionTestResult> TestConnectionAsync(AIProviderRequestContext ctx, CancellationToken ct = default);
    public abstract Task<string> AnalyzeImageAsync(BitmapSource image, string prompt, AIProviderRequestContext ctx, CancellationToken ct = default);

    public virtual async Task<AIAnalysisResult> GenerateGuideAsync(BitmapSource image, string prompt, AIProviderRequestContext ctx, CancellationToken ct = default)
        => ParseResult(await AnalyzeImageAsync(image, prompt, ctx, ct));

    public virtual async Task<AIAnalysisResult> GenerateAnnotationsAsync(BitmapSource image, string prompt, AIProviderRequestContext ctx, CancellationToken ct = default)
        => ParseResult(await AnalyzeImageAsync(image, prompt, ctx, ct));

    protected static string ToBase64Png(BitmapSource image) => ImageHelper.ToBase64Png(image);

    protected static string TrimEndpoint(string endpoint) => endpoint.TrimEnd('/');

    /// <summary>
    /// Pulls the first balanced JSON object out of arbitrary model text. Trusts
    /// only structured JSON; never depends on free-form prose.
    /// </summary>
    public static AIAnalysisResult ParseResult(string content)
    {
        var json = ExtractJson(content);
        if (string.IsNullOrWhiteSpace(json))
            return new AIAnalysisResult { Text = content };

        try
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<AIAnalysisResult>(json, opts) ?? new AIAnalysisResult { Text = content };
        }
        catch
        {
            return new AIAnalysisResult { Text = content };
        }
    }

    public static string ExtractJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start) return string.Empty;
        return text.Substring(start, end - start + 1);
    }

    protected static string ReadString(JsonElement el, params string[] path)
    {
        var current = el;
        foreach (var p in path)
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(p, out current))
                return string.Empty;
        }
        return current.ValueKind == JsonValueKind.String ? current.GetString() ?? string.Empty : current.ToString();
    }
}
