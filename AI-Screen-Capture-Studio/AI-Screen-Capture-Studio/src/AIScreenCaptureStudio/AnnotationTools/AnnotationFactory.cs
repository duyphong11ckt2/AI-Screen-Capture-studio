using AIScreenCaptureStudio.Models;
using AIScreenCaptureStudio.Models.Annotations;

namespace AIScreenCaptureStudio.AnnotationTools;

/// <summary>Creates blank annotations by tool, or editable objects from AI specs.</summary>
public static class AnnotationFactory
{
    public static AnnotationBase Create(EditorTool tool, double x, double y) => tool switch
    {
        EditorTool.Arrow => new ArrowAnnotation { X = x, Y = y },
        EditorTool.Rectangle => new RectangleAnnotation { X = x, Y = y },
        EditorTool.Highlight => new HighlightAnnotation { X = x, Y = y },
        EditorTool.Text => new TextAnnotation { X = x, Y = y },
        EditorTool.NumberedStep => new NumberedStepAnnotation { X = x, Y = y },
        _ => new RectangleAnnotation { X = x, Y = y }
    };

    public static AnnotationBase FromSpec(AIAnnotationSpec spec, int stepNumber)
    {
        AnnotationBase a = spec.Type.ToLowerInvariant() switch
        {
            "arrow" => new ArrowAnnotation(),
            "rectangle" or "rect" or "box" => new RectangleAnnotation(),
            "highlight" => new HighlightAnnotation(),
            "text" or "label" => new TextAnnotation { Text = spec.Label },
            "step" or "number" or "numberedstep" => new NumberedStepAnnotation { Number = stepNumber },
            _ => new RectangleAnnotation()
        };

        a.X = spec.X;
        a.Y = spec.Y;
        a.Width = spec.Width > 0 ? spec.Width : a.Width;
        a.Height = spec.Height > 0 ? spec.Height : a.Height;
        a.Label = spec.Label;
        a.Name = string.IsNullOrWhiteSpace(spec.Label) ? a.Name : spec.Label;
        a.Confidence = Math.Clamp(spec.Confidence, 0, 1);
        a.IsAIGenerated = true;
        return a;
    }
}
