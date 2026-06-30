namespace AIScreenCaptureStudio.Models.Annotations;

public sealed class RectangleAnnotation : AnnotationBase
{
    public override AnnotationType Type => AnnotationType.Rectangle;

    public RectangleAnnotation()
    {
        Name = "Rectangle";
    }

    public override AnnotationBase Clone()
    {
        var c = new RectangleAnnotation();
        CopyBaseTo(c);
        return c;
    }
}
