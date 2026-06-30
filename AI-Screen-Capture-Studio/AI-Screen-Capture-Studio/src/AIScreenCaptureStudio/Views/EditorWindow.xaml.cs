using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.AnnotationTools;
using AIScreenCaptureStudio.Models;
using AIScreenCaptureStudio.Models.Annotations;
using AIScreenCaptureStudio.Services;
using AIScreenCaptureStudio.ViewModels;

namespace AIScreenCaptureStudio.Views;

public partial class EditorWindow : Window
{
    private enum DragMode { None, Draw, Move, Resize }

    private DragMode _mode = DragMode.None;
    private AnnotationBase? _active;
    private Point _startPoint;
    private Rect _beforeRect;

    public EditorWindow() => InitializeComponent();

    private EditorViewModel Vm => (EditorViewModel)DataContext;

    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var p = e.GetPosition(InteractionLayer);
        var vm = Vm;

        if (vm.CurrentTool == EditorTool.Select)
        {
            var hit = HitTest(p);
            vm.SelectedAnnotation = hit;
            if (hit is not null)
            {
                _beforeRect = ToRect(hit);
                _startPoint = p;
                _mode = IsInResizeZone(hit, p) ? DragMode.Resize : DragMode.Move;
                InteractionLayer.CaptureMouse();
            }
            return;
        }

        // Drawing tools
        _active = vm.AddUserAnnotation(vm.CurrentTool, p);
        _startPoint = p;

        if (_active is TextAnnotation or NumberedStepAnnotation)
        {
            // Point annotations are placed immediately.
            vm.CurrentTool = EditorTool.Select;
            _active = null;
            _mode = DragMode.None;
        }
        else
        {
            _active.Width = 1;
            _active.Height = 1;
            _mode = DragMode.Draw;
            InteractionLayer.CaptureMouse();
        }
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_mode == DragMode.None) return;
        var p = e.GetPosition(InteractionLayer);

        switch (_mode)
        {
            case DragMode.Draw when _active is not null:
                _active.X = Math.Min(p.X, _startPoint.X);
                _active.Y = Math.Min(p.Y, _startPoint.Y);
                _active.Width = Math.Max(1, Math.Abs(p.X - _startPoint.X));
                _active.Height = Math.Max(1, Math.Abs(p.Y - _startPoint.Y));
                break;

            case DragMode.Move when Vm.SelectedAnnotation is { } sel:
                sel.X += p.X - _startPoint.X;
                sel.Y += p.Y - _startPoint.Y;
                _startPoint = p;
                break;

            case DragMode.Resize when Vm.SelectedAnnotation is { } r:
                r.Width = Math.Max(4, p.X - r.X);
                r.Height = Math.Max(4, p.Y - r.Y);
                break;
        }
    }

    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        InteractionLayer.ReleaseMouseCapture();

        if (_mode == DragMode.Draw)
        {
            Vm.CurrentTool = EditorTool.Select;
        }
        else if (_mode is DragMode.Move or DragMode.Resize && Vm.SelectedAnnotation is { } sel)
        {
            Vm.RecordTransform(sel, _beforeRect, ToRect(sel));
        }

        _mode = DragMode.None;
        _active = null;
    }

    private AnnotationBase? HitTest(Point p)
    {
        // Topmost first: user layer above AI layer.
        foreach (var layer in new[] { Vm.UserLayer, Vm.AILayer })
        {
            if (!layer.IsVisible) continue;
            for (var i = layer.Annotations.Count - 1; i >= 0; i--)
            {
                var a = layer.Annotations[i];
                if (ToRect(a).Contains(p)) return a;
            }
        }
        return null;
    }

    private static bool IsInResizeZone(AnnotationBase a, Point p)
    {
        var corner = new Point(a.X + a.Width, a.Y + a.Height);
        return Math.Abs(p.X - corner.X) <= 14 && Math.Abs(p.Y - corner.Y) <= 14;
    }

    private static Rect ToRect(AnnotationBase a) => new(a.X, a.Y, a.Width, a.Height);

    /// <summary>Render the visible image + annotation layers (no selection chrome).</summary>
    public BitmapSource RenderFlattened()
    {
        var keep = Vm.SelectedAnnotation;
        Vm.SelectedAnnotation = null;
        FlattenRoot.UpdateLayout();

        var bmp = ImageHelper.RenderVisual(FlattenRoot, Vm.ImagePixelWidth, Vm.ImagePixelHeight);

        Vm.SelectedAnnotation = keep;
        return bmp;
    }
}
