using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using AIScreenCaptureStudio.Models;
using AIScreenCaptureStudio.Mvvm;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.ViewModels;

public sealed class HistoryViewModel : ObservableObject
{
    private readonly IHistoryService _history;
    private readonly IClipboardService _clipboard;
    private readonly IDialogService _dialog;

    public HistoryViewModel(IHistoryService history, IClipboardService clipboard, IDialogService dialog)
    {
        _history = history;
        _clipboard = clipboard;
        _dialog = dialog;

        Reload();
        RefreshCommand = new RelayCommand(_ => Reload());
        DeleteCommand = new RelayCommand(_ => Delete(), _ => Selected is not null);
        CopyResponseCommand = new RelayCommand(_ => CopyResponse(), _ => Selected is not null);
        ExportCommand = new RelayCommand(_ => Export(), _ => Selected is not null);
    }

    public ObservableCollection<HistoryEntry> Entries { get; } = new();

    private HistoryEntry? _selected;
    public HistoryEntry? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                OnPropertyChanged(nameof(SelectedThumbnail));
                OnPropertyChanged(nameof(SelectedResponse));
            }
        }
    }

    public BitmapSource? SelectedThumbnail => Selected is null ? null : _history.LoadThumbnail(Selected);
    public string SelectedResponse => Selected?.RawResponse ?? string.Empty;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand CopyResponseCommand { get; }
    public RelayCommand ExportCommand { get; }

    public void Reload()
    {
        Entries.Clear();
        foreach (var e in _history.GetAll()) Entries.Add(e);
    }

    private void Delete()
    {
        if (Selected is null) return;
        if (!_dialog.Confirm("Delete this history entry?")) return;
        _history.Delete(Selected.Id);
        Reload();
    }

    private void CopyResponse()
    {
        if (Selected is not null) _clipboard.CopyText(Selected.RawResponse);
    }

    private void Export()
    {
        if (Selected is null) return;
        var path = _dialog.ShowSaveFileDialog("JSON file|*.json", $"history_{Selected.Id}.json");
        if (path is not null)
        {
            _history.Export(Selected, path);
            _dialog.ShowMessage("Exported.");
        }
    }
}
