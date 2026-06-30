using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Models;

namespace AIScreenCaptureStudio.Providers;

/// <summary>
/// High-level façade the view models use. Resolves the active provider from
/// settings, decrypts the API key, runs the prompt, and persists history.
/// </summary>
public interface IAIService
{
    Task<ConnectionTestResult> TestConnectionAsync(CancellationToken ct = default);
    Task<AIAnalysisResult> GenerateGuidelineAsync(BitmapSource image, CancellationToken ct = default);
    Task<AIAnalysisResult> GenerateAnnotationsAsync(BitmapSource image, CancellationToken ct = default);
    Task<string> RunPromptAsync(BitmapSource image, PromptType promptType, CancellationToken ct = default);
}
