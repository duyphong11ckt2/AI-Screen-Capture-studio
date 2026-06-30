using AIScreenCaptureStudio.Configuration;

namespace AIScreenCaptureStudio.Providers;

/// <summary>
/// Resolves a provider implementation by type. Providers are registered in DI;
/// adding a new provider means registering it and adding one switch arm — no UI
/// changes required.
/// </summary>
public sealed class AIProviderFactory : IAIProviderFactory
{
    private readonly IEnumerable<IAIProvider> _providers;

    public AIProviderFactory(IEnumerable<IAIProvider> providers) => _providers = providers;

    public IAIProvider Create(AIProviderType type)
    {
        var match = _providers.FirstOrDefault(p => p.ProviderType == type);
        return match ?? throw new NotSupportedException($"No provider registered for {type}.");
    }
}
