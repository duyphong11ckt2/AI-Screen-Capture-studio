using AIScreenCaptureStudio.Mvvm;

namespace AIScreenCaptureStudio.Models.Annotations;

public sealed class TextAnnotation : AnnotationBase
{
    private string _text = "Text";
    private double _fontSize = 16;

    public override AnnotationType Type => AnnotationType.Text;

    public TextAnnotation()
    {
        Name = "Text";
        Height = 28;
    }

    public string Text { get => _text; set => SetProperty(ref _text, value); }
    public double FontSize { get => _fontSize; set => SetProperty(ref _fontSize, value); }

    public override AnnotationBase Clone()
    {
        var c = new TextAnnotation { Text = Text, FontSize = FontSize };
        CopyBaseTo(c);
        return c;
    }
}
