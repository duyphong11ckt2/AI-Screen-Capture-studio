namespace AIScreenCaptureStudio.Services.Interfaces;

public interface IDialogService
{
    string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDir = null);
    string? ShowFolderBrowserDialog(string? initialDir = null);
    void ShowMessage(string message, string title = "AI Screen Capture Studio");
    bool Confirm(string message, string title = "AI Screen Capture Studio");
}
