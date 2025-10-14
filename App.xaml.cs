using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GhostOverlay.Helpers;
using GhostOverlay.Services;
using GhostOverlay.ViewModels;
using GhostOverlay.Views;
using Serilog;
using Application = System.Windows.Application;

namespace GhostOverlay;

public partial class App : Application
{
    private IHost? _host;
    private OverlayWindow? _overlayWindow;
    private HotkeyHelper? _hotkeyHelper;
    private TrayIconHelper? _trayHelper;
    private MonitoringService? _monitoringService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Setup logging
        SetupLogging();

        // Setup DI
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<ConfigService>();
                services.AddSingleton<HttpMonitorService>();
                services.AddSingleton<PingMonitorService>();
                services.AddSingleton<TcpMonitorService>();
                services.AddSingleton<MonitoringService>();
                services.AddSingleton<NotificationService>();

                // ViewModels
                services.AddSingleton<OverlayViewModel>();

                // HttpClient
                services.AddHttpClient<HttpMonitorService>();

                // Logging
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddSerilog(dispose: true);
                });
            })
            .Build();

        // Create overlay window
        _overlayWindow = new OverlayWindow();
        var viewModel = _host.Services.GetRequiredService<OverlayViewModel>();
        _overlayWindow.DataContext = viewModel;

        // Setup hotkey (Ctrl+Shift+O)
        var configService = _host.Services.GetRequiredService<ConfigService>();
        _hotkeyHelper = new HotkeyHelper(
            _overlayWindow,
            configService.Settings.HotkeyModifiers,
            configService.Settings.HotkeyKey
        );
        _hotkeyHelper.HotkeyPressed += (s, e) => ToggleOverlay();

        // Setup tray icon
        _trayHelper = new TrayIconHelper(_overlayWindow);

        // Setup notification service
        var notificationService = _host.Services.GetRequiredService<NotificationService>();
        notificationService.SetNotifyIcon(_trayHelper.NotifyIcon);

        // Start monitoring service
        _monitoringService = _host.Services.GetRequiredService<MonitoringService>();
        _monitoringService.Start();

        // Show overlay if not starting minimized
        if (!configService.Settings.StartMinimized)
        {
            _overlayWindow.Show();
        }
    }

    private void ToggleOverlay()
    {
        if (_overlayWindow == null) return;

        if (_overlayWindow.IsVisible)
        {
            _overlayWindow.Hide();
        }
        else
        {
            _overlayWindow.Show();
            _overlayWindow.Activate();
        }
    }

    private void SetupLogging()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var logPath = Path.Combine(appData, "GhostOverlay", "logs", "app-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7
            )
            .CreateLogger();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _monitoringService?.Stop();
        _hotkeyHelper?.Dispose();
        _trayHelper?.Dispose();
        _host?.Dispose();
        Log.CloseAndFlush();

        base.OnExit(e);
    }
}

