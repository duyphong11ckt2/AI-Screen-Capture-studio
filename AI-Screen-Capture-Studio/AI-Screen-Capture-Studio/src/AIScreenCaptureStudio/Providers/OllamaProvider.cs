using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Models;

namespace AIScreenCaptureStudio.Providers;

/// <summary>Local Ollama server (no API key required).</summary>
public sealed class OllamaProvider : AIProviderBase
{
    public OllamaProvider(HttpClient http) : base(http) { }

    public override AIProviderType ProviderType => AIProviderType.Ollama;
    public override string DisplayName => "Ollama (local)";

    private static string ChatUrl(string endpoint) => $"{TrimEndpoint(endpoint)}/api/chat";
    private static string TagsUrl(string endpoint) => $"{TrimEndpoint(endpoint)}/api/tags";

    public override async Task<ConnectionTestResult> TestConnectionAsync(AIProviderRequestContext ctx, CancellationToken ct = default)
    {
        try
        {
            using var res = await Http.GetAsync(TagsUrl(ctx.Endpoint), ct);
            if (res.IsSuccessStatusCode) return ConnectionTestResult.Ok();
            return ConnectionTestResult.Fail($"{(int)res.StatusCode} {res.ReasonPhrase}");
        }
        catch (Exception ex) { return ConnectionTestResult.Fail(ex.Message); }
    }

    public override async Task<string> AnalyzeImageAsync(BitmapSource image, string prompt, AIProviderRequestContext ctx, CancellationToken ct = default)
    {
        var payload = new
        {
            model = ctx.Model,
            stream = false,
            messages = new object[]
            {
                new { role = "user", content = prompt, images = new[] { ToBase64Png(image) } }
            }
        };

        using var res = await Http.PostAsJsonAsync(ChatUrl(ctx.Endpoint), payload, ct);
        var json = await res.Content.ReadAsStringAsync(ct);
        res.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
    }
}
