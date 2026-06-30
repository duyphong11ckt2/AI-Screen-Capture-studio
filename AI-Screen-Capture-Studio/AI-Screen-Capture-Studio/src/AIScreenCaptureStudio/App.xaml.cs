using System.Net.Http;
using System.Windows;
using System.Windows.Threading;
using AIScreenCaptureStudio.Configuration;
using AIScreenCaptureStudio.Providers;
using AIScreenCaptureStudio.Services;
using AIScreenCaptureStudio.Services.Interfaces;
using AIScreenCaptureStudio.ViewModels;
using AIScreenCaptureStudio.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AIScreenCaptureStudio;

public partial class App : Application
{
    private IServiceProvider _services = null!;
    public static IServiceProvider Services => ((App)Current)._services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var sc = new ServiceCollection();
        ConfigureServices(sc);
        _services = sc.BuildServiceProvider();

        ApplyTheme(_services.GetRequiredService<IStorageService>().Settings.Theme);

        // The main window hosts the tray icon and the hotkey message loop.
        var main = _services.GetRequiredService<MainWindow>();
        main.DataContext = _services.GetRequiredService<MainViewModel>();
        MainWindow = main;
        main.Show();   // initializes the HWND; the window hides itself to tray.

        DispatcherUnhandledException += OnUnhandledException;
    }

    private static void ConfigureServices(IServiceCollection sc)
    {
        // Cross-cutting
        sc.AddSingleton<HttpClient>(_ => new HttpClient { Timeout = TimeSpan.FromSeconds(120) });

        // Core services
        sc.AddSingleton<IStorageService, StorageService>();
        sc.AddSingleton<ISecureStorageService, SecureStorageService>();
        sc.AddSingleton<ICaptureService, CaptureService>();
        sc.AddSingleton<IClipboardService, ClipboardService>();
        sc.AddSingleton<IExportService, ExportService>();
        sc.AddSingleton<IOcrService, OcrService>();
        sc.AddSingleton<IHotkeyService, HotkeyService>();
        sc.AddSingleton<IHistoryService, HistoryService>();
        sc.AddSingleton<IDialogService, DialogService>();
        sc.AddSingleton<IWindowManager, WindowManager>();

        // AI providers (register every implementation as IAIProvider)
        sc.AddSingleton<IAIProvider, OpenAIProvider>();
        sc.AddSingleton<IAIProvider, ClaudeProvider>();
        sc.AddSingleton<IAIProvider, GeminiProvider>();
        sc.AddSingleton<IAIProvider, OllamaProvider>();
        sc.AddSingleton<IAIProvider, CustomOpenAICompatibleProvider>();
        sc.AddSingleton<IAIProviderFactory, AIProviderFactory>();
        sc.AddSingleton<IAIService, AIService>();

        // View models
        sc.AddSingleton<MainViewModel>();
        sc.AddTransient<SettingsViewModel>();
        sc.AddTransient<AISettingsViewModel>();
        sc.AddTransient<HistoryViewModel>();

        // Windows
        sc.AddSingleton<MainWindow>();
        sc.AddTransient<SettingsWindow>();
        sc.AddTransient<HistoryWindow>();
    }

    public static void ApplyTheme(AppTheme theme)
    {
        var uri = theme == AppTheme.Light
            ? new Uri("Resources/Light.xaml", UriKind.Relative)
            : new Uri("Resources/Dark.xaml", UriKind.Relative);
        var dict = new ResourceDictionary { Source = uri };
        var dicts = Current.Resources.MergedDictionaries;
        if (dicts.Count > 0) dicts[0] = dict;
        else dicts.Add(dict);
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
