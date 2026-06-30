using AIScreenCaptureStudio.Configuration;

namespace AIScreenCaptureStudio.Providers;

public interface IAIProviderFactory
{
    IAIProvider Create(AIProviderType type);
}
