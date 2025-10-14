using System.IO;
using System.Windows;
using System.Windows.Forms;
using GhostOverlay.Views;
using Application = System.Windows.Application;

namespace GhostOverlay.Helpers;

public class TrayIconHelper : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly OverlayWindow _overlayWindow;

    public TrayIconHelper(OverlayWindow overlayWindow)
    {
        _overlayWindow = overlayWindow;

        _notifyIcon = new NotifyIcon
        {
            Text = "Overlay Monitor",
            Visible = true
        };

        // Try to load icon, fallback to application icon
        try
        {
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "tray-icon.ico");
            if (File.Exists(iconPath))
            {
                _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
            }
            else
            {
                // Use default application icon
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }
        }
        catch
        {
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
        }

        // Context menu
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show Overlay", null, (s, e) => ShowOverlay());
        contextMenu.Items.Add("Settings", null, (s, e) => OpenSettings());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (s, e) => ShowOverlay();

        // Show balloon on startup
        _notifyIcon.ShowBalloonTip(
            3000,
            "Overlay Monitor",
            "Monitoring started. Press Ctrl+Shift+O to toggle overlay.",
            ToolTipIcon.Info
        );
    }

    public NotifyIcon NotifyIcon => _notifyIcon;

    public void UpdateStatus(string status)
    {
        _notifyIcon.Text = $"Overlay Monitor - {status}";
    }

    public void ShowNotification(string title, string message, ToolTipIcon icon)
    {
        _notifyIcon.ShowBalloonTip(5000, title, message, icon);
    }

    private void ShowOverlay()
    {
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

    private void OpenSettings()
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.ShowDialog();
    }

    private void ExitApplication()
    {
        Application.Current.Shutdown();
    }

    public void Dispose()
    {
        _notifyIcon?.Dispose();
    }
}
