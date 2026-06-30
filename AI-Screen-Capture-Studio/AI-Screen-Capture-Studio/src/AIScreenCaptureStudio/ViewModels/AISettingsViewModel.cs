using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Mvvm;
using AIScreenCaptureStudio.Providers;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.ViewModels;

/// <summary>
/// Edits the AI provider configuration and prompt templates. The plain-text API
/// key lives only in memory here; <see cref="Apply"/> encrypts it via DPAPI.
/// </summary>
public sealed class AISettingsViewModel : ObservableObject
{
    private readonly IStorageService _storage;
    private readonly ISecureStorageService _secure;
    private readonly IAIService _ai;
    private readonly IDialogService _dialog;

    public AISettingsViewModel(IStorageService storage, ISecureStorageService secure,
        IAIService ai, IDialogService dialog)
    {
        _storage = storage;
        _secure = secure;
        _ai = ai;
        _dialog = dialog;

        var cfg = storage.Settings.AI;
        _provider = cfg.Provider;
        _model = cfg.Model;
        _endpoint = cfg.Endpoint;
        _apiKey = secure.Decrypt(cfg.EncryptedApiKey);

        var p = storage.Settings.Prompts;
        _guidelinePrompt = p.GuidelineGenerator;
        _annotationPrompt = p.AnnotationGenerator;
        _functionPrompt = p.FunctionDetection;
        _ocrPrompt = p.OcrAnalyzer;

        TestConnectionCommand = new AsyncRelayCommand(_ => TestConnectionAsync(), () => !IsTesting);
        UseProviderDefaultsCommand = new RelayCommand(_ => ApplyProviderDefaults());
        RestoreGuidelineCommand = new RelayCommand(_ => GuidelinePrompt = PromptTemplates.DefaultGuideline);
        RestoreAnnotationCommand = new RelayCommand(_ => AnnotationPrompt = PromptTemplates.DefaultAnnotation);
        RestoreFunctionCommand = new RelayCommand(_ => FunctionPrompt = PromptTemplates.DefaultFunctionDetection);
        RestoreOcrCommand = new RelayCommand(_ => OcrPrompt = PromptTemplates.DefaultOcr);
    }

    public Array Providers => Enum.GetValues(typeof(AIProviderType));

    private AIProviderType _provider;
    public AIProviderType Provider
    {
        get => _provider;
        set { if (SetProperty(ref _provider, value)) OnPropertyChanged(nameof(RequiresApiKey)); }
    }

    public bool RequiresApiKey => Provider != AIProviderType.Ollama;

    private string _model;
    public string Model { get => _model; set => SetProperty(ref _model, value); }

    private string _endpoint;
    public string Endpoint { get => _endpoint; set => SetProperty(ref _endpoint, value); }

    private string _apiKey;
    public string ApiKey { get => _apiKey; set => SetProperty(ref _apiKey, value); }

    private bool _isTesting;
    public bool IsTesting { get => _isTesting; set => SetProperty(ref _isTesting, value); }

    private string _testStatus = string.Empty;
    public string TestStatus { get => _testStatus; set => SetProperty(ref _testStatus, value); }

    private string _guidelinePrompt;
    public string GuidelinePrompt { get => _guidelinePrompt; set => SetProperty(ref _guidelinePrompt, value); }
    private string _annotationPrompt;
    public string AnnotationPrompt { get => _annotationPrompt; set => SetProperty(ref _annotationPrompt, value); }
    private string _functionPrompt;
    public string FunctionPrompt { get => _functionPrompt; set => SetProperty(ref _functionPrompt, value); }
    private string _ocrPrompt;
    public string OcrPrompt { get => _ocrPrompt; set => SetProperty(ref _ocrPrompt, value); }

    public AsyncRelayCommand TestConnectionCommand { get; }
    public RelayCommand UseProviderDefaultsCommand { get; }
    public RelayCommand RestoreGuidelineCommand { get; }
    public RelayCommand RestoreAnnotationCommand { get; }
    public RelayCommand RestoreFunctionCommand { get; }
    public RelayCommand RestoreOcrCommand { get; }

    private void ApplyProviderDefaults()
    {
        (Endpoint, Model) = Provider switch
        {
            AIProviderType.OpenAI => ("https://api.openai.com/v1", "gpt-4o"),
            AIProviderType.Claude => ("https://api.anthropic.com", "claude-sonnet-4-6"),
            AIProviderType.Gemini => ("https://generativelanguage.googleapis.com", "gemini-1.5-pro"),
            AIProviderType.Ollama => ("http://localhost:11434", "llama3.2-vision"),
            _ => (Endpoint, Model)
        };
    }

    private async Task TestConnectionAsync()
    {
        try
        {
            IsTesting = true;
            TestStatus = "Testing…";
            Apply();                 // persist so IAIService picks up the new values
            var result = await _ai.TestConnectionAsync();
            TestStatus = result.Success ? $"✓ {result.Message}" : $"✗ {result.Message}";
        }
        catch (Exception ex) { TestStatus = $"✗ {ex.Message}"; }
        finally { IsTesting = false; }
    }

    /// <summary>Writes the in-memory edits back into settings, encrypting the key.</summary>
    public void Apply()
    {
        var cfg = _storage.Settings.AI;
        cfg.Provider = Provider;
        cfg.Model = Model;
        cfg.Endpoint = Endpoint;
        cfg.EncryptedApiKey = string.IsNullOrEmpty(ApiKey) ? string.Empty : _secure.Encrypt(ApiKey);

        var p = _storage.Settings.Prompts;
        p.GuidelineGenerator = GuidelinePrompt;
        p.AnnotationGenerator = AnnotationPrompt;
        p.FunctionDetection = FunctionPrompt;
        p.OcrAnalyzer = OcrPrompt;

        _storage.Save();
    }
}
