using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Models;

namespace AIScreenCaptureStudio.Providers;

public sealed class GeminiProvider : AIProviderBase
{
    public GeminiProvider(HttpClient http) : base(http) { }

    public override AIProviderType ProviderType => AIProviderType.Gemini;
    public override string DisplayName => "Gemini (Google)";

    private static string GenerateUrl(string endpoint, string model, string key) =>
        $"{TrimEndpoint(endpoint)}/v1beta/models/{model}:generateContent?key={key}";

    private static string ModelsUrl(string endpoint, string key) =>
        $"{TrimEndpoint(endpoint)}/v1beta/models?key={key}";

    public override async Task<ConnectionTestResult> TestConnectionAsync(AIProviderRequestContext ctx, CancellationToken ct = default)
    {
        try
        {
            using var res = await Http.GetAsync(ModelsUrl(ctx.Endpoint, ctx.ApiKey), ct);
            if (res.IsSuccessStatusCode) return ConnectionTestResult.Ok();
            var body = await res.Content.ReadAsStringAsync(ct);
            return ConnectionTestResult.Fail($"{(int)res.StatusCode} {res.ReasonPhrase}: {Trunc(body)}");
        }
        catch (Exception ex) { return ConnectionTestResult.Fail(ex.Message); }
    }

    public override async Task<string> AnalyzeImageAsync(BitmapSource image, string prompt, AIProviderRequestContext ctx, CancellationToken ct = default)
    {
        var payload = new
        {
            contents = new object[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = prompt },
                        new { inline_data = new { mime_type = "image/png", data = ToBase64Png(image) } }
                    }
                }
            }
        };

        using var res = await Http.PostAsJsonAsync(GenerateUrl(ctx.Endpoint, ctx.Model, ctx.ApiKey), payload, ct);
        var json = await res.Content.ReadAsStringAsync(ct);
        res.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(json);
        var sb = new System.Text.StringBuilder();
        foreach (var cand in doc.RootElement.GetProperty("candidates").EnumerateArray())
        {
            if (cand.TryGetProperty("content", out var c) && c.TryGetProperty("parts", out var parts))
                foreach (var part in parts.EnumerateArray())
                    if (part.TryGetProperty("text", out var t)) sb.Append(t.GetString());
        }
        return sb.ToString();
    }

    private static string Trunc(string s) => s.Length > 300 ? s[..300] : s;
}
