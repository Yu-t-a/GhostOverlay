using GhostOverlay.Models;

namespace GhostOverlay.Services;

public interface IMonitorService
{
    Task<MonitorResult> CheckAsync(Endpoint endpoint, CancellationToken ct = default);
}
