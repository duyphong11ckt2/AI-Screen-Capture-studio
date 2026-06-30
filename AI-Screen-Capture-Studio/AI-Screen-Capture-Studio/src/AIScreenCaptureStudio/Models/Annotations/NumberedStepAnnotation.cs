using AIScreenCaptureStudio.Mvvm;

namespace AIScreenCaptureStudio.Models.Annotations;

public sealed class NumberedStepAnnotation : AnnotationBase
{
    private int _number = 1;

    public override AnnotationType Type => AnnotationType.NumberedStep;

    public NumberedStepAnnotation()
    {
        Name = "Step";
        Width = 36; Height = 36;
        StrokeColor = System.Windows.Media.Color.FromRgb(0x53, 0x4A, 0xB7);
    }

    public int Number { get => _number; set => SetProperty(ref _number, value); }

    public override AnnotationBase Clone()
    {
        var c = new NumberedStepAnnotation { Number = Number };
        CopyBaseTo(c);
        return c;
    }
}
