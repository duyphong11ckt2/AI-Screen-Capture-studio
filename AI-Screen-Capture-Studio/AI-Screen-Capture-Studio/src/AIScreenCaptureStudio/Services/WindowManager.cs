using System.Windows;
using AIScreenCaptureStudio.Models;
using AIScreenCaptureStudio.Services.Interfaces;
using AIScreenCaptureStudio.ViewModels;
using AIScreenCaptureStudio.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AIScreenCaptureStudio.Services;

/// <summary>
/// Resolves and shows top-level windows so view models never reference WPF
/// window types directly.
/// </summary>
public sealed class WindowManager : IWindowManager
{
    private readonly IServiceProvider _sp;

    public WindowManager(IServiceProvider sp) => _sp = sp;

    public void ShowEditor(CaptureResult capture)
    {
        var vm = ActivatorUtilities.CreateInstance<EditorViewModel>(_sp, capture);
        var win = new EditorWindow { DataContext = vm };
        vm.FlattenProvider = win.RenderFlattened;
        win.Show();
        win.Activate();
    }

    public void ShowSettings()
    {
        var win = _sp.GetRequiredService<SettingsWindow>();
        win.DataContext = _sp.GetRequiredService<SettingsViewModel>();
        win.Owner = ActiveOrNull();
        win.ShowDialog();
    }

    public void ShowHistory()
    {
        var win = _sp.GetRequiredService<HistoryWindow>();
        win.DataContext = _sp.GetRequiredService<HistoryViewModel>();
        win.Show();
        win.Activate();
    }

    public void ShowMain()
    {
        var main = _sp.GetRequiredService<MainWindow>();
        main.ShowFromTray();
    }

    public Int32Rect? ShowRegionSelector()
    {
        var win = new RegionSelectionWindow();
        var ok = win.ShowDialog();
        return ok == true ? win.SelectedRegion : null;
    }

    private static Window? ActiveOrNull()
        => Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
}
