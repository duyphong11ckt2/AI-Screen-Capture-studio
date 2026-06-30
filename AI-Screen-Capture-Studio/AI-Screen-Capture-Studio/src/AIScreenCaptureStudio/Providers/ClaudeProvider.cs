using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Models;

namespace AIScreenCaptureStudio.Providers;

public sealed class ClaudeProvider : AIProviderBase
{
    public ClaudeProvider(HttpClient http) : base(http) { }

    public override AIProviderType ProviderType => AIProviderType.Claude;
    public override string DisplayName => "Claude (Anthropic)";

    private static string MessagesUrl(string endpoint) => $"{TrimEndpoint(endpoint)}/v1/messages";

    public override async Task<ConnectionTestResult> TestConnectionAsync(AIProviderRequestContext ctx, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                model = ctx.Model,
                max_tokens = 1,
                messages = new object[] { new { role = "user", content = "ping" } }
            };
            using var req = BuildRequest(payload, ctx);
            using var res = await Http.SendAsync(req, ct);
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
                        new
                        {
                            type = "image",
                            source = new { type = "base64", media_type = "image/png", data = ToBase64Png(image) }
                        }
                    }
                }
            }
        };

        using var req = BuildRequest(payload, ctx);
        using var res = await Http.SendAsync(req, ct);
        var json = await res.Content.ReadAsStringAsync(ct);
        res.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(json);
        var content = doc.RootElement.GetProperty("content");
        var sb = new System.Text.StringBuilder();
        foreach (var block in content.EnumerateArray())
        {
            if (block.TryGetProperty("type", out var t) && t.GetString() == "text" &&
                block.TryGetProperty("text", out var txt))
                sb.Append(txt.GetString());
        }
        return sb.ToString();
    }

    private static HttpRequestMessage BuildRequest(object payload, AIProviderRequestContext ctx)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, MessagesUrl(ctx.Endpoint))
        {
            Content = JsonContent.Create(payload)
        };
        req.Headers.Add("x-api-key", ctx.ApiKey);
        req.Headers.Add("anthropic-version", "2023-06-01");
        return req;
    }

    private static string Trunc(string s) => s.Length > 300 ? s[..300] : s;
}
