using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using GhostOverlay.Models;

namespace GhostOverlay.Services;

public class HttpMonitorService : IMonitorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpMonitorService> _logger;

    public HttpMonitorService(HttpClient httpClient, ILogger<HttpMonitorService> logger)
    {
        _httpClient = httpClient;
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
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(endpoint.TimeoutMs);

            var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, endpoint.Target),
                HttpCompletionOption.ResponseHeadersRead,
                cts.Token
            );

            sw.Stop();
            result.LatencyMs = (int)sw.ElapsedMilliseconds;
            result.HttpStatusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                result.Status = result.LatencyMs > endpoint.SlowThresholdMs
                    ? EndpointStatus.Slow
                    : EndpointStatus.Up;
            }
            else if ((int)response.StatusCode >= 500)
            {
                result.Status = EndpointStatus.Down;
                result.ErrorMessage = $"HTTP {response.StatusCode}";
            }
            else
            {
                result.Status = EndpointStatus.Slow;
                result.ErrorMessage = $"HTTP {response.StatusCode}";
            }
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            result.Status = EndpointStatus.Down;
            result.ErrorMessage = "Timeout";
            result.LatencyMs = endpoint.TimeoutMs;
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            result.Status = EndpointStatus.Down;
            result.ErrorMessage = ex.Message;
            result.LatencyMs = (int)sw.ElapsedMilliseconds;
        }

        _logger.LogInformation(
            "Checked {Target}: {Status} ({LatencyMs}ms)",
            endpoint.Target,
            result.Status,
            result.LatencyMs
        );

        return result;
    }
}
