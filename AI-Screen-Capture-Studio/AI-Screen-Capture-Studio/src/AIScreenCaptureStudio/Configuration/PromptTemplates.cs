namespace AIScreenCaptureStudio.Configuration;

public enum PromptType
{
    GuidelineGenerator,
    OcrAnalyzer,
    FunctionDetection,
    AnnotationGenerator
}

/// <summary>
/// User-editable prompt templates with restorable defaults. The defaults
/// instruct the model to return strict JSON matching the app's schema.
/// </summary>
public sealed class PromptTemplates
{
    public string GuidelineGenerator { get; set; } = DefaultGuideline;
    public string OcrAnalyzer { get; set; } = DefaultOcr;
    public string FunctionDetection { get; set; } = DefaultFunctionDetection;
    public string AnnotationGenerator { get; set; } = DefaultAnnotation;

    public string Get(PromptType type) => type switch
    {
        PromptType.GuidelineGenerator => GuidelineGenerator,
        PromptType.OcrAnalyzer => OcrAnalyzer,
        PromptType.FunctionDetection => FunctionDetection,
        PromptType.AnnotationGenerator => AnnotationGenerator,
        _ => GuidelineGenerator
    };

    public void Set(PromptType type, string value)
    {
        switch (type)
        {
            case PromptType.GuidelineGenerator: GuidelineGenerator = value; break;
            case PromptType.OcrAnalyzer: OcrAnalyzer = value; break;
            case PromptType.FunctionDetection: FunctionDetection = value; break;
            case PromptType.AnnotationGenerator: AnnotationGenerator = value; break;
        }
    }

    public string Default(PromptType type) => type switch
    {
        PromptType.GuidelineGenerator => DefaultGuideline,
        PromptType.OcrAnalyzer => DefaultOcr,
        PromptType.FunctionDetection => DefaultFunctionDetection,
        PromptType.AnnotationGenerator => DefaultAnnotation,
        _ => DefaultGuideline
    };

    public const string DefaultGuideline =
        "You are a senior technical writer. Analyze the attached screenshot of a software UI. " +
        "Identify interactive elements (buttons, menus, inputs, dropdowns, tabs, tables, links, icons, grids, forms). " +
        "Produce a concise, step-by-step end-user guide. " +
        "Respond with STRICT JSON only, no markdown, no prose outside JSON, matching this schema: " +
        "{\"functions\":[{\"name\":string,\"confidence\":number}]," +
        "\"steps\":[{\"step\":number,\"description\":string}]," +
        "\"annotations\":[{\"type\":\"arrow|rectangle|highlight|text|step\",\"label\":string,\"x\":number,\"y\":number,\"width\":number,\"height\":number,\"confidence\":number}]}. " +
        "Coordinates are pixels relative to the top-left of the image. confidence is 0..1.";

    public const string DefaultOcr =
        "Extract all readable text from the screenshot, preserving reading order. " +
        "Respond with STRICT JSON only: {\"text\":string,\"blocks\":[{\"text\":string,\"x\":number,\"y\":number,\"width\":number,\"height\":number}]}.";

    public const string DefaultFunctionDetection =
        "Detect the distinct user-facing functions available in this UI screenshot. " +
        "Respond with STRICT JSON only: {\"functions\":[{\"name\":string,\"confidence\":number}]}. confidence is 0..1.";

    public const string DefaultAnnotation =
        "Detect interactive UI elements and produce editable annotations that point at them. " +
        "Respond with STRICT JSON only: {\"annotations\":[{\"type\":\"arrow|rectangle|highlight|text|step\",\"label\":string,\"x\":number,\"y\":number,\"width\":number,\"height\":number,\"confidence\":number}]}. " +
        "Coordinates are pixels relative to the top-left of the image. confidence is 0..1. " +
        "Prefer numbered 'step' annotations ordered by typical usage flow.";
}
