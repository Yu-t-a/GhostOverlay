using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using GhostOverlay.Models;

namespace GhostOverlay.Services;

public class TcpMonitorService : IMonitorService
{
    private readonly ILogger<TcpMonitorService> _logger;

    public TcpMonitorService(ILogger<TcpMonitorService> logger)
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

        var sw = Stopwatch.StartNew();

        try
        {
            // Parse host:port from target
            var parts = endpoint.Target.Split(':');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Target must be in format 'host:port'");
            }

            var host = parts[0];
            var port = int.Parse(parts[1]);

            using var tcpClient = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(endpoint.TimeoutMs);

            await tcpClient.ConnectAsync(host, port, cts.Token);

            sw.Stop();
            result.LatencyMs = (int)sw.ElapsedMilliseconds;
            result.Status = result.LatencyMs > endpoint.SlowThresholdMs
                ? EndpointStatus.Slow
                : EndpointStatus.Up;

            _logger.LogInformation(
                "TCP connected to {Target}: {Status} ({LatencyMs}ms)",
                endpoint.Target,
                result.Status,
                result.LatencyMs
            );
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.Status = EndpointStatus.Down;
            result.ErrorMessage = ex.Message;
            result.LatencyMs = (int)sw.ElapsedMilliseconds;

            _logger.LogError(ex, "Failed to connect to {Target}", endpoint.Target);
        }

        return result;
    }
}
