using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Models;

namespace AIScreenCaptureStudio.Providers;

public class OpenAIProvider : AIProviderBase
{
    public OpenAIProvider(HttpClient http) : base(http) { }

    public override AIProviderType ProviderType => AIProviderType.OpenAI;
    public override string DisplayName => "OpenAI";

    protected virtual string ChatUrl(string endpoint) => $"{TrimEndpoint(endpoint)}/chat/completions";
    protected virtual string ModelsUrl(string endpoint) => $"{TrimEndpoint(endpoint)}/models";

    public override async Task<ConnectionTestResult> TestConnectionAsync(AIProviderRequestContext ctx, CancellationToken ct = default)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, ModelsUrl(ctx.Endpoint));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.ApiKey);
            using var res = await Http.SendAsync(req, ct);
            if (res.IsSuccessStatusCode) return ConnectionTestResult.Ok();
            var body = await res.Content.ReadAsStringAsync(ct);
            return ConnectionTestResult.Fail($"{(int)res.StatusCode} {res.ReasonPhrase}: {Truncate(body)}");
        }
        catch (Exception ex) { return ConnectionTestResult.Fail(ex.Message); }
    }

    public override async Task<string> AnalyzeImageAsync(BitmapSource image, string prompt, AIProviderRequestContext ctx, CancellationToken ct = default)
    {
        var dataUrl = $"data:image/png;base64,{ToBase64Png(image)}";
        var payload = new
        {
            model = ctx.Model,
            max_tokens = 2000,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt },
                        new { type = "image_url", image_url = new { url = dataUrl } }
                    }
                }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, ChatUrl(ctx.Endpoint))
        {
            Content = JsonContent.Create(payload)
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.ApiKey);

        using var res = await Http.SendAsync(req, ct);
        var json = await res.Content.ReadAsStringAsync(ct);
        res.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }

    protected static string Truncate(string s) => s.Length > 300 ? s[..300] : s;
}
