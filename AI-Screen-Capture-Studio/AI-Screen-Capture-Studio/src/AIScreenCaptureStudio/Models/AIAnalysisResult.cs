using System.Text.Json.Serialization;

namespace AIScreenCaptureStudio.Models;

/// <summary>
/// Strongly typed deserialization target for the structured JSON returned by
/// every AI provider. Free-form text responses are never trusted.
/// </summary>
public sealed class AIAnalysisResult
{
    [JsonPropertyName("functions")]
    public List<DetectedFunction> Functions { get; set; } = new();

    [JsonPropertyName("steps")]
    public List<GuideStep> Steps { get; set; } = new();

    [JsonPropertyName("annotations")]
    public List<AIAnnotationSpec> Annotations { get; set; } = new();

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public sealed class DetectedFunction
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("confidence")] public double Confidence { get; set; } = 1.0;
}

public sealed class GuideStep
{
    [JsonPropertyName("step")] public int Step { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
}

public sealed class AIAnnotationSpec
{
    [JsonPropertyName("type")] public string Type { get; set; } = "rectangle";
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("x")] public double X { get; set; }
    [JsonPropertyName("y")] public double Y { get; set; }
    [JsonPropertyName("width")] public double Width { get; set; } = 120;
    [JsonPropertyName("height")] public double Height { get; set; } = 60;
    [JsonPropertyName("confidence")] public double Confidence { get; set; } = 1.0;
}
