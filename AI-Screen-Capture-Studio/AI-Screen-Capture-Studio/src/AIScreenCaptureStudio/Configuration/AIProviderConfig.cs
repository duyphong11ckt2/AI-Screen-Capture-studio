namespace AIScreenCaptureStudio.Configuration;

/// <summary>
/// Persisted AI provider configuration. The API key is never stored in plain
/// text: only <see cref="EncryptedApiKey"/> (DPAPI ciphertext) is serialized.
/// </summary>
public sealed class AIProviderConfig
{
    public AIProviderType Provider { get; set; } = AIProviderType.OpenAI;
    public string Model { get; set; } = "gpt-4o";
    public string Endpoint { get; set; } = "https://api.openai.com/v1";

    /// <summary>Base64 DPAPI-encrypted API key. Written by SecureStorageService.</summary>
    public string EncryptedApiKey { get; set; } = string.Empty;

    /// <summary>Per-provider remembered endpoints/models for quick switching.</summary>
    public Dictionary<string, ProviderProfile> Profiles { get; set; } = new();
}

public sealed class ProviderProfile
{
    public string Endpoint { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string EncryptedApiKey { get; set; } = string.Empty;
}
