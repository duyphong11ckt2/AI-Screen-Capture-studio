using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.AnnotationTools;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Models;
using AIScreenCaptureStudio.Models.Annotations;
using AIScreenCaptureStudio.Mvvm;
using AIScreenCaptureStudio.Providers;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.ViewModels;

public sealed class EditorViewModel : ObservableObject
{
    private readonly IAIService _ai;
    private readonly IExportService _export;
    private readonly IClipboardService _clipboard;
    private readonly IOcrService _ocr;
    private readonly IStorageService _storage;
    private readonly IDialogService _dialog;
    private readonly UndoRedoStack _history = new();

    private BitmapSource _imageSource = null!;
    private EditorTool _currentTool = EditorTool.Select;
    private AnnotationBase? _selectedAnnotation;
    private double _zoom = 1.0;
    private string _statusText = "Ready";
    private bool _isBusy;
    private string _ocrText = string.Empty;
    private AnnotationBase? _clipboardAnnotation;

    public EditorViewModel(CaptureResult capture, IAIService ai, IExportService export,
        IClipboardService clipboard, IOcrService ocr, IStorageService storage, IDialogService dialog)
    {
        _ai = ai; _export = export; _clipboard = clipboard;
        _ocr = ocr; _storage = storage; _dialog = dialog;

        _imageSource = capture.Image;
        BuildLayers();
        InitCommands();
    }

    public BitmapSource ImageSource { get => _imageSource; private set => SetProperty(ref _imageSource, value); }
    public int ImagePixelWidth => ImageSource.PixelWidth;
    public int ImagePixelHeight => ImageSource.PixelHeight;

    public ObservableCollection<Layer> Layers { get; } = new();
    public Layer OriginalLayer => Layers.First(l => l.Kind == LayerKind.OriginalScreenshot);
    public Layer AILayer => Layers.First(l => l.Kind == LayerKind.AIAnnotations);
    public Layer UserLayer => Layers.First(l => l.Kind == LayerKind.UserAnnotations);

    public ReviewViewModel Review { get; } = new();

    /// <summary>Set by the view; renders the flattened editor canvas to a bitmap.</summary>
    public Func<BitmapSource>? FlattenProvider { get; set; }

    public EditorTool CurrentTool
    {
        get => _currentTool;
        set { if (SetProperty(ref _currentTool, value)) OnPropertyChanged(nameof(IsSelectTool)); }
    }
    public bool IsSelectTool => CurrentTool == EditorTool.Select;

    public AnnotationBase? SelectedAnnotation
    {
        get => _selectedAnnotation;
        set
        {
            if (_selectedAnnotation is not null) _selectedAnnotation.IsSelected = false;
            if (SetProperty(ref _selectedAnnotation, value) && value is not null) value.IsSelected = true;
            OnPropertyChanged(nameof(HasSelection));
        }
    }
    public bool HasSelection => SelectedAnnotation is not null;

    public double Zoom { get => _zoom; set => SetProperty(ref _zoom, Math.Clamp(value, 0.1, 8.0)); }
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
    public string OcrText { get => _ocrText; set => SetProperty(ref _ocrText, value); }

    public RelayCommand SelectToolCommand { get; private set; } = null!;
    public RelayCommand DeleteSelectedCommand { get; private set; } = null!;
    public RelayCommand CopyAnnotationCommand { get; private set; } = null!;
    public RelayCommand PasteAnnotationCommand { get; private set; } = null!;
    public RelayCommand UndoCommand { get; private set; } = null!;
    public RelayCommand RedoCommand { get; private set; } = null!;
    public RelayCommand ZoomInCommand { get; private set; } = null!;
    public RelayCommand ZoomOutCommand { get; private set; } = null!;
    public RelayCommand FitCommand { get; private set; } = null!;
    public RelayCommand CopyImageCommand { get; private set; } = null!;
    public RelayCommand SaveAsCommand { get; private set; } = null!;
    public AsyncRelayCommand GenerateGuidelineCommand { get; private set; } = null!;
    public AsyncRelayCommand GenerateAnnotationsCommand { get; private set; } = null!;
    public AsyncRelayCommand RunOcrCommand { get; private set; } = null!;
    public RelayCommand AcceptAllCommand { get; private set; } = null!;
    public RelayCommand RejectAllCommand { get; private set; } = null!;

    private void BuildLayers()
    {
        Layers.Add(new Layer { Kind = LayerKind.OriginalScreenshot, Name = "Original screenshot", ZIndex = 0, IsLocked = true });
        Layers.Add(new Layer { Kind = LayerKind.AIAnnotations, Name = "AI annotations", ZIndex = 1 });
        Layers.Add(new Layer { Kind = LayerKind.UserAnnotations, Name = "User annotations", ZIndex = 2 });
    }

    private void InitCommands()
    {
        SelectToolCommand = new RelayCommand(p =>
        {
            if (p is EditorTool t) CurrentTool = t;
            else if (p is string s && Enum.TryParse<EditorTool>(s, out var et)) CurrentTool = et;
        });

        DeleteSelectedCommand = new RelayCommand(_ => DeleteSelected(), _ => HasSelection);
        CopyAnnotationCommand = new RelayCommand(_ => _clipboardAnnotation = SelectedAnnotation?.Clone(), _ => HasSelection);
        PasteAnnotationCommand = new RelayCommand(_ => PasteAnnotation(), _ => _clipboardAnnotation is not null);

        UndoCommand = new RelayCommand(_ => { _history.Undo(); Refresh(); }, _ => _history.CanUndo);
        RedoCommand = new RelayCommand(_ => { _history.Redo(); Refresh(); }, _ => _history.CanRedo);

        ZoomInCommand = new RelayCommand(_ => Zoom *= 1.2);
        ZoomOutCommand = new RelayCommand(_ => Zoom /= 1.2);
        FitCommand = new RelayCommand(_ => Zoom = 1.0);

        CopyImageCommand = new RelayCommand(_ => CopyFlattenedToClipboard());
        SaveAsCommand = new RelayCommand(_ => SaveAs());

        GenerateGuidelineCommand = new AsyncRelayCommand(_ => GenerateGuidelineAsync(), () => !IsBusy);
        GenerateAnnotationsCommand = new AsyncRelayCommand(_ => GenerateAnnotationsAsync(), () => !IsBusy);
        RunOcrCommand = new AsyncRelayCommand(_ => RunOcrAsync(), () => !IsBusy);

        AcceptAllCommand = new RelayCommand(_ => ApplyReviewedAnnotations(), _ => Review.HasResult);
        RejectAllCommand = new RelayCommand(_ => Review.Clear(), _ => Review.HasResult);
    }

    // ---- annotation editing -------------------------------------------------

    public AnnotationBase AddUserAnnotation(EditorTool tool, Point at)
    {
        var ann = AnnotationFactory.Create(tool, at.X, at.Y);
        var layer = UserLayer;
        _history.Push(new UndoableAction
        {
            Label = "Add annotation",
            Do = () => { if (!layer.Annotations.Contains(ann)) layer.Annotations.Add(ann); },
            Undo = () => layer.Annotations.Remove(ann)
        });
        SelectedAnnotation = ann;
        return ann;
    }

    private void DeleteSelected()
    {
        var ann = SelectedAnnotation;
        if (ann is null) return;
        var layer = Layers.FirstOrDefault(l => l.Annotations.Contains(ann));
        if (layer is null || layer.IsLocked) return;
        _history.Push(new UndoableAction
        {
            Label = "Delete annotation",
            Do = () => layer.Annotations.Remove(ann),
            Undo = () => layer.Annotations.Add(ann)
        });
        SelectedAnnotation = null;
    }

    private void PasteAnnotation()
    {
        if (_clipboardAnnotation is null) return;
        var clone = _clipboardAnnotation.Clone();
        clone.X += 20; clone.Y += 20;
        var layer = UserLayer;
        _history.Push(new UndoableAction
        {
            Label = "Paste annotation",
            Do = () => layer.Annotations.Add(clone),
            Undo = () => layer.Annotations.Remove(clone)
        });
        SelectedAnnotation = clone;
    }

    /// <summary>Record a completed move/resize so it can be undone.</summary>
    public void RecordTransform(AnnotationBase ann, Rect before, Rect after)
    {
        if (before == after) return;
        _history.Record(new UndoableAction
        {
            Label = "Transform",
            Do = () => Apply(ann, after),
            Undo = () => Apply(ann, before)
        });
        Refresh();
    }

    private static void Apply(AnnotationBase a, Rect r)
    {
        a.X = r.X; a.Y = r.Y; a.Width = r.Width; a.Height = r.Height;
    }

    // ---- AI -----------------------------------------------------------------

    private async Task GenerateGuidelineAsync()
    {
        try
        {
            IsBusy = true; StatusText = "Generating guideline…";
            var result = await _ai.GenerateGuidelineAsync(ImageSource);
            Review.Load(result);
            StatusText = $"Guideline ready: {Review.Functions.Count} functions, {Review.Steps.Count} steps";
        }
        catch (Exception ex) { StatusText = $"AI error: {ex.Message}"; _dialog.ShowMessage(ex.Message, "Generate guideline failed"); }
        finally { IsBusy = false; }
    }

    private async Task GenerateAnnotationsAsync()
    {
        try
        {
            IsBusy = true; StatusText = "Generating annotations…";
            var result = await _ai.GenerateAnnotationsAsync(ImageSource);
            Review.Load(result);
            StatusText = $"{Review.Annotations.Count} annotations proposed — review before applying";
        }
        catch (Exception ex) { StatusText = $"AI error: {ex.Message}"; _dialog.ShowMessage(ex.Message, "Generate annotations failed"); }
        finally { IsBusy = false; }
    }

    private void ApplyReviewedAnnotations()
    {
        var specs = Review.Annotations.ToList();
        var step = 1;
        var created = new List<AnnotationBase>();
        foreach (var s in specs)
        {
            var a = AnnotationFactory.FromSpec(s, step);
            if (a is NumberedStepAnnotation) step++;
            created.Add(a);
        }
        var layer = AILayer;
        _history.Push(new UndoableAction
        {
            Label = "Apply AI annotations",
            Do = () => { foreach (var a in created) layer.Annotations.Add(a); },
            Undo = () => { foreach (var a in created) layer.Annotations.Remove(a); }
        });
        StatusText = $"Applied {created.Count} AI annotations (editable on AI layer)";
        Review.Clear();
    }

    private async Task RunOcrAsync()
    {
        try
        {
            IsBusy = true; StatusText = "Running OCR…";
            var res = await _ocr.ExtractTextAsync(ImageSource);
            OcrText = res.Text;
            StatusText = _ocr.IsAvailable ? $"OCR done ({Math.Round(res.MeanConfidence * 100)}% confidence)" : "OCR unavailable";
        }
        catch (Exception ex) { StatusText = $"OCR error: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    public void CopyOcr() => _clipboard.CopyText(OcrText);

    // ---- export -------------------------------------------------------------

    private void CopyFlattenedToClipboard()
    {
        var bmp = FlattenProvider?.Invoke() ?? ImageSource;
        _clipboard.CopyImage(bmp);
        StatusText = "Copied flattened image to clipboard";
    }

    private void SaveAs()
    {
        var fmt = _storage.Settings.DefaultExportFormat;
        var ext = fmt.ToString().ToLowerInvariant();
        var name = $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.{ext}";
        var path = _dialog.ShowSaveFileDialog(
            "PNG image|*.png|JPEG image|*.jpg|PDF document|*.pdf",
            name, _storage.Settings.DefaultSaveFolder);
        if (path is null) return;

        var actualFormat = System.IO.Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => ExportFormat.Jpg,
            ".pdf" => ExportFormat.Pdf,
            _ => ExportFormat.Png
        };
        var bmp = FlattenProvider?.Invoke() ?? ImageSource;
        _export.Save(bmp, path, actualFormat);
        StatusText = $"Saved {actualFormat} → {path}";
    }

    private void Refresh()
    {
        OnPropertyChanged(nameof(HasSelection));
        UndoCommand.RaiseCanExecuteChanged();
        RedoCommand.RaiseCanExecuteChanged();
    }
}
