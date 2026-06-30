using System.Windows.Media;
using AIScreenCaptureStudio.Mvvm;

namespace AIScreenCaptureStudio.Models.Annotations;

/// <summary>
/// Base class for every editable annotation object. All annotations live on a
/// layer and expose bindable geometry so the editor canvas can move/resize them
/// without ever burning pixels onto the underlying screenshot.
/// </summary>
public abstract class AnnotationBase : ObservableObject
{
    private double _x, _y, _width = 120, _height = 60;
    private string _name = string.Empty;
    private string _label = string.Empty;
    private bool _isSelected;
    private double _confidence = 1.0;
    private bool _isAIGenerated;
    private Color _strokeColor = Colors.OrangeRed;
    private double _strokeThickness = 2.0;

    public Guid Id { get; init; } = Guid.NewGuid();

    public abstract AnnotationType Type { get; }

    public double X { get => _x; set => SetProperty(ref _x, value); }
    public double Y { get => _y; set => SetProperty(ref _y, value); }
    public double Width { get => _width; set => SetProperty(ref _width, Math.Max(1, value)); }
    public double Height { get => _height; set => SetProperty(ref _height, Math.Max(1, value)); }

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Label { get => _label; set => SetProperty(ref _label, value); }

    public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

    /// <summary>0..1 detection confidence (1.0 for user-created objects).</summary>
    public double Confidence { get => _confidence; set => SetProperty(ref _confidence, value); }

    public bool IsAIGenerated { get => _isAIGenerated; set => SetProperty(ref _isAIGenerated, value); }

    public Color StrokeColor { get => _strokeColor; set { if (SetProperty(ref _strokeColor, value)) OnPropertyChanged(nameof(StrokeBrush)); } }
    public double StrokeThickness { get => _strokeThickness; set => SetProperty(ref _strokeThickness, value); }

    public Brush StrokeBrush => new SolidColorBrush(StrokeColor);

    /// <summary>True when confidence is below the low-confidence threshold.</summary>
    public bool IsLowConfidence => IsAIGenerated && Confidence < 0.75;

    public abstract AnnotationBase Clone();

    protected void CopyBaseTo(AnnotationBase target)
    {
        target.X = X; target.Y = Y; target.Width = Width; target.Height = Height;
        target.Name = Name; target.Label = Label; target.Confidence = Confidence;
        target.IsAIGenerated = IsAIGenerated; target.StrokeColor = StrokeColor;
        target.StrokeThickness = StrokeThickness;
    }
}
