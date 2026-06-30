using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AIScreenCaptureStudio.Views;

public partial class RegionSelectionWindow : Window
{
    private Point _start;
    private bool _dragging;

    public Int32Rect SelectedRegion { get; private set; }

    public RegionSelectionWindow() => InitializeComponent();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
        Canvas.SetLeft(Hint, 24);
        Canvas.SetTop(Hint, 24);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { DialogResult = false; Close(); }
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _start = e.GetPosition(Root);
        _dragging = true;
        SelectionBox.Visibility = Visibility.Visible;
        Canvas.SetLeft(SelectionBox, _start.X);
        Canvas.SetTop(SelectionBox, _start.Y);
        SelectionBox.Width = 0;
        SelectionBox.Height = 0;
        CaptureMouse();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_dragging) return;
        var p = e.GetPosition(Root);
        var x = Math.Min(p.X, _start.X);
        var y = Math.Min(p.Y, _start.Y);
        Canvas.SetLeft(SelectionBox, x);
        Canvas.SetTop(SelectionBox, y);
        SelectionBox.Width = Math.Abs(p.X - _start.X);
        SelectionBox.Height = Math.Abs(p.Y - _start.Y);
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_dragging) return;
        _dragging = false;
        ReleaseMouseCapture();

        var x = Canvas.GetLeft(SelectionBox);
        var y = Canvas.GetTop(SelectionBox);
        var w = SelectionBox.Width;
        var h = SelectionBox.Height;

        if (w < 2 || h < 2) { DialogResult = false; Close(); return; }

        // Convert DIP overlay coordinates to absolute device pixels.
        var dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
        var deviceLeft = (int)Math.Round((Left + x) * dpi);
        var deviceTop = (int)Math.Round((Top + y) * dpi);
        var deviceW = (int)Math.Round(w * dpi);
        var deviceH = (int)Math.Round(h * dpi);

        SelectedRegion = new Int32Rect(deviceLeft, deviceTop, Math.Max(1, deviceW), Math.Max(1, deviceH));
        DialogResult = true;
        Close();
    }
}
