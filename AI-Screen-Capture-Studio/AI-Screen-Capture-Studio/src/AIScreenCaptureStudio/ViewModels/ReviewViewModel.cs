using System.Collections.ObjectModel;
using AIScreenCaptureStudio.Models;

namespace AIScreenCaptureStudio.ViewModels;

/// <summary>
/// Backs the review panel/window. Holds the parsed AI result so the user can
/// inspect functions, steps, and proposed annotations before applying them.
/// </summary>
public sealed class ReviewViewModel : Mvvm.ObservableObject
{
    private bool _hasResult;

    public ObservableCollection<DetectedFunction> Functions { get; } = new();
    public ObservableCollection<GuideStep> Steps { get; } = new();
    public ObservableCollection<AIAnnotationSpec> Annotations { get; } = new();

    public bool HasResult { get => _hasResult; set => SetProperty(ref _hasResult, value); }

    public void Load(AIAnalysisResult result)
    {
        Functions.Clear();
        Steps.Clear();
        Annotations.Clear();
        foreach (var f in result.Functions) Functions.Add(f);
        foreach (var s in result.Steps.OrderBy(s => s.Step)) Steps.Add(s);
        foreach (var a in result.Annotations) Annotations.Add(a);
        HasResult = Functions.Count + Steps.Count + Annotations.Count > 0;
    }

    public void Clear()
    {
        Functions.Clear(); Steps.Clear(); Annotations.Clear();
        HasResult = false;
    }
}
