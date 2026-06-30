using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Models;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.Providers;

/// <summary>
/// Orchestrates the active provider: builds the request context from settings
/// (decrypting the API key via DPAPI), runs prompts, and records history.
/// </summary>
public sealed class AIService : IAIService
{
    private readonly IAIProviderFactory _factory;
    private readonly IStorageService _storage;
    private readonly ISecureStorageService _secure;
    private readonly IHistoryService _history;

    public AIService(IAIProviderFactory factory, IStorageService storage,
        ISecureStorageService secure, IHistoryService history)
    {
        _factory = factory;
        _storage = storage;
        _secure = secure;
        _history = history;
    }

    private (IAIProvider provider, AIProviderRequestContext ctx) Resolve()
    {
        var cfg = _storage.Settings.AI;
        var provider = _factory.Create(cfg.Provider);
        var ctx = new AIProviderRequestContext
        {
            Endpoint = cfg.Endpoint,
            Model = cfg.Model,
            ApiKey = _secure.Decrypt(cfg.EncryptedApiKey)
        };
        return (provider, ctx);
    }

    public Task<ConnectionTestResult> TestConnectionAsync(CancellationToken ct = default)
    {
        var (provider, ctx) = Resolve();
        return provider.TestConnectionAsync(ctx, ct);
    }

    public async Task<AIAnalysisResult> GenerateGuidelineAsync(BitmapSource image, CancellationToken ct = default)
    {
        var (provider, ctx) = Resolve();
        var prompt = _storage.Settings.Prompts.GuidelineGenerator;
        var raw = await provider.AnalyzeImageAsync(image, prompt, ctx, ct);
        Record(prompt, raw, image);
        return AIProviderBase.ParseResult(raw);
    }

    public async Task<AIAnalysisResult> GenerateAnnotationsAsync(BitmapSource image, CancellationToken ct = default)
    {
        var (provider, ctx) = Resolve();
        var prompt = _storage.Settings.Prompts.AnnotationGenerator;
        var raw = await provider.AnalyzeImageAsync(image, prompt, ctx, ct);
        Record(prompt, raw, image);
        return AIProviderBase.ParseResult(raw);
    }

    public async Task<string> RunPromptAsync(BitmapSource image, PromptType promptType, CancellationToken ct = default)
    {
        var (provider, ctx) = Resolve();
        var prompt = _storage.Settings.Prompts.Get(promptType);
        var raw = await provider.AnalyzeImageAsync(image, prompt, ctx, ct);
        Record(prompt, raw, image);
        return raw;
    }

    private void Record(string prompt, string raw, BitmapSource image)
    {
        var cfg = _storage.Settings.AI;
        _history.Add(new HistoryEntry
        {
            Provider = cfg.Provider,
            Model = cfg.Model,
            Prompt = prompt,
            RawResponse = raw
        }, image);
    }
}
