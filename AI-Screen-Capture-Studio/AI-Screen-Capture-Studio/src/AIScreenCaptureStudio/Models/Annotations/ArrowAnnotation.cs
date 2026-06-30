namespace AIScreenCaptureStudio.Models.Annotations;

public sealed class ArrowAnnotation : AnnotationBase
{
    public override AnnotationType Type => AnnotationType.Arrow;

    public ArrowAnnotation()
    {
        Name = "Arrow";
        Width = 120; Height = 60;
    }

    public override AnnotationBase Clone()
    {
        var c = new ArrowAnnotation();
        CopyBaseTo(c);
        return c;
    }
}
