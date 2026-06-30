using System.Windows;
using System.Windows.Controls;
using AIScreenCaptureStudio.Models.Annotations;

namespace AIScreenCaptureStudio.Converters;

/// <summary>Picks the right DataTemplate per annotation type on the editor canvas.</summary>
public sealed class AnnotationTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ArrowTemplate { get; set; }
    public DataTemplate? RectangleTemplate { get; set; }
    public DataTemplate? HighlightTemplate { get; set; }
    public DataTemplate? TextTemplate { get; set; }
    public DataTemplate? StepTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container) => item switch
    {
        ArrowAnnotation => ArrowTemplate,
        RectangleAnnotation => RectangleTemplate,
        HighlightAnnotation => HighlightTemplate,
        TextAnnotation => TextTemplate,
        NumberedStepAnnotation => StepTemplate,
        _ => base.SelectTemplate(item, container)
    };
}
