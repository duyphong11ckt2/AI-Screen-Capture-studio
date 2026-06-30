using System.Collections.ObjectModel;
using AIScreenCaptureStudio.Models.Annotations;
using AIScreenCaptureStudio.Mvvm;

namespace AIScreenCaptureStudio.Models;

public enum LayerKind
{
    OriginalScreenshot,
    AIAnnotations,
    UserAnnotations
}

/// <summary>
/// A layer in the editor stack. The original screenshot layer holds no
/// annotations; the AI and user layers hold editable annotation objects.
/// </summary>
public sealed class Layer : ObservableObject
{
    private string _name = string.Empty;
    private bool _isVisible = true;
    private bool _isLocked;
    private int _zIndex;

    public Guid Id { get; } = Guid.NewGuid();
    public LayerKind Kind { get; init; }

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
    public bool IsLocked { get => _isLocked; set => SetProperty(ref _isLocked, value); }
    public int ZIndex { get => _zIndex; set => SetProperty(ref _zIndex, value); }

    public ObservableCollection<AnnotationBase> Annotations { get; } = new();
}
