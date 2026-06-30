using System.Windows;
using AIScreenCaptureStudio.Services.Interfaces;
using Microsoft.Win32;

namespace AIScreenCaptureStudio.Services;

public sealed class DialogService : IDialogService
{
    public string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDir = null)
    {
        var dlg = new SaveFileDialog
        {
            Filter = filter,
            FileName = defaultFileName,
            InitialDirectory = initialDir ?? string.Empty,
            OverwritePrompt = true
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? ShowFolderBrowserDialog(string? initialDir = null)
    {
        var dlg = new OpenFolderDialog
        {
            InitialDirectory = initialDir ?? string.Empty,
            Multiselect = false
        };
        return dlg.ShowDialog() == true ? dlg.FolderName : null;
    }

    public void ShowMessage(string message, string title = "AI Screen Capture Studio")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public bool Confirm(string message, string title = "AI Screen Capture Studio")
        => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
