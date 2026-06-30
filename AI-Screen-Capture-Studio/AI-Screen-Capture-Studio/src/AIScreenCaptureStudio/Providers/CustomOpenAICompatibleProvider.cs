using AIScreenCaptureStudio.Configuration;

namespace AIScreenCaptureStudio.Providers;

/// <summary>
/// Any OpenAI-compatible endpoint (LM Studio, vLLM, OpenRouter, Together, etc.).
/// Reuses the OpenAI wire format but reports itself as the Custom provider.
/// </summary>
public sealed class CustomOpenAICompatibleProvider : OpenAIProvider
{
    public CustomOpenAICompatibleProvider(HttpClient http) : base(http) { }

    public override AIProviderType ProviderType => AIProviderType.Custom;
    public override string DisplayName => "Custom (OpenAI-compatible)";
}
