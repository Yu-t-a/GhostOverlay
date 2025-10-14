using System.Windows.Input;

namespace GhostOverlay.Models;

public class AppSettings
{
    public ModifierKeys HotkeyModifiers { get; set; } = ModifierKeys.Control | ModifierKeys.Shift;
    public Key HotkeyKey { get; set; } = Key.O;
    public int PollIntervalSeconds { get; set; } = 60;
    public int SlowThresholdMs { get; set; } = 800;
    public string Theme { get; set; } = "Dark";
    public List<Endpoint> Endpoints { get; set; } = new();
    public bool ShowNotifications { get; set; } = true;
    public bool StartMinimized { get; set; } = true;
    public bool AutoStartMonitoring { get; set; } = true;
}
