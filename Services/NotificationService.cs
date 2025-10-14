using System.Windows.Forms;
using GhostOverlay.Models;

namespace GhostOverlay.Services;

public class NotificationService
{
    private readonly Dictionary<Guid, DateTime> _lastNotifications = new();
    private readonly TimeSpan _debounceTime = TimeSpan.FromMinutes(5);
    private NotifyIcon? _notifyIcon;

    public void SetNotifyIcon(NotifyIcon notifyIcon)
    {
        _notifyIcon = notifyIcon;
    }

    public void NotifyStatusChange(Endpoint endpoint, EndpointStatus oldStatus, EndpointStatus newStatus)
    {
        // Only notify on status change
        if (oldStatus == newStatus)
            return;

        // Debounce notifications
        if (_lastNotifications.TryGetValue(endpoint.Id, out var lastTime))
        {
            if (DateTime.Now - lastTime < _debounceTime)
                return;
        }

        // Show notification
        var icon = newStatus switch
        {
            EndpointStatus.Down => ToolTipIcon.Error,
            EndpointStatus.Slow => ToolTipIcon.Warning,
            EndpointStatus.Up => ToolTipIcon.Info,
            _ => ToolTipIcon.None
        };

        var message = newStatus switch
        {
            EndpointStatus.Down => $"{endpoint.Name} is DOWN",
            EndpointStatus.Slow => $"{endpoint.Name} is SLOW",
            EndpointStatus.Up => $"{endpoint.Name} is UP",
            _ => $"{endpoint.Name} status unknown"
        };

        ShowToast("Server Status Change", message, icon);

        _lastNotifications[endpoint.Id] = DateTime.Now;
    }

    public void ShowToast(string title, string message, ToolTipIcon icon)
    {
        _notifyIcon?.ShowBalloonTip(5000, title, message, icon);
    }
}
