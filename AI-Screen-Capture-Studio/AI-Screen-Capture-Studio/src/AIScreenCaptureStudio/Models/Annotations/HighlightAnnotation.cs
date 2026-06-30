using System.Windows.Media;
using AIScreenCaptureStudio.Mvvm;

namespace AIScreenCaptureStudio.Models.Annotations;

public sealed class HighlightAnnotation : AnnotationBase
{
    private Color _fillColor = Color.FromArgb(60, 0xEF, 0x9F, 0x27);
    private double _fillOpacity = 0.30;

    public override AnnotationType Type => AnnotationType.Highlight;

    public HighlightAnnotation()
    {
        Name = "Highlight";
        StrokeColor = Color.FromRgb(0xBA, 0x75, 0x17);
    }

    public Color FillColor { get => _fillColor; set { if (SetProperty(ref _fillColor, value)) OnPropertyChanged(nameof(FillBrush)); } }
    public double FillOpacity { get => _fillOpacity; set => SetProperty(ref _fillOpacity, value); }
    public Brush FillBrush => new SolidColorBrush(FillColor);

    public override AnnotationBase Clone()
    {
        var c = new HighlightAnnotation { FillColor = FillColor, FillOpacity = FillOpacity };
        CopyBaseTo(c);
        return c;
    }
}
