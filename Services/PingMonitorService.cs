using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using GhostOverlay.Models;

namespace GhostOverlay.Services;

public class PingMonitorService : IMonitorService
{
    private readonly ILogger<PingMonitorService> _logger;

    public PingMonitorService(ILogger<PingMonitorService> logger)
    {
        _logger = logger;
    }

    public async Task<MonitorResult> CheckAsync(Endpoint endpoint, CancellationToken ct = default)
    {
        var result = new MonitorResult
        {
            EndpointId = endpoint.Id,
            LastChecked = DateTime.UtcNow
        };

        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(
                endpoint.Target,
                endpoint.TimeoutMs
            );

            if (reply.Status == IPStatus.Success)
            {
                result.LatencyMs = (int)reply.RoundtripTime;
                result.Status = result.LatencyMs > endpoint.SlowThresholdMs
                    ? EndpointStatus.Slow
                    : EndpointStatus.Up;
            }
            else
            {
                result.Status = EndpointStatus.Down;
                result.ErrorMessage = reply.Status.ToString();
            }

            _logger.LogInformation(
                "Pinged {Target}: {Status} ({LatencyMs}ms)",
                endpoint.Target,
                result.Status,
                result.LatencyMs
            );
        }
        catch (Exception ex)
        {
            result.Status = EndpointStatus.Down;
            result.ErrorMessage = ex.Message;

            _logger.LogError(ex, "Failed to ping {Target}", endpoint.Target);
        }

        return result;
    }
}
