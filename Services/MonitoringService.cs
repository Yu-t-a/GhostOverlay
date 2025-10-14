using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using GhostOverlay.Models;
using Timer = System.Threading.Timer;

namespace GhostOverlay.Services;

public class MonitoringService
{
    private readonly ConfigService _configService;
    private readonly HttpMonitorService _httpMonitor;
    private readonly PingMonitorService _pingMonitor;
    private readonly TcpMonitorService _tcpMonitor;
    private readonly NotificationService _notificationService;
    private readonly ILogger<MonitoringService> _logger;

    private readonly ConcurrentDictionary<Guid, MonitorResult> _results = new();
    private readonly ConcurrentDictionary<Guid, Timer> _timers = new();
    private CancellationTokenSource? _cts;

    public event EventHandler<MonitorResult>? ResultUpdated;

    public MonitoringService(
        ConfigService configService,
        HttpMonitorService httpMonitor,
        PingMonitorService pingMonitor,
        TcpMonitorService tcpMonitor,
        NotificationService notificationService,
        ILogger<MonitoringService> logger)
    {
        _configService = configService;
        _httpMonitor = httpMonitor;
        _pingMonitor = pingMonitor;
        _tcpMonitor = tcpMonitor;
        _notificationService = notificationService;
        _logger = logger;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _logger.LogInformation("Starting monitoring service...");

        foreach (var endpoint in _configService.Settings.Endpoints.Where(e => e.IsEnabled))
        {
            StartMonitoring(endpoint);
        }
    }

    public void Stop()
    {
        _logger.LogInformation("Stopping monitoring service...");
        _cts?.Cancel();

        foreach (var timer in _timers.Values)
        {
            timer.Dispose();
        }

        _timers.Clear();
    }

    private void StartMonitoring(Endpoint endpoint)
    {
        // Check immediately
        _ = CheckEndpointAsync(endpoint);

        // Then schedule periodic checks
        var timer = new Timer(
            async _ => await CheckEndpointAsync(endpoint),
            null,
            TimeSpan.FromSeconds(endpoint.IntervalSeconds),
            TimeSpan.FromSeconds(endpoint.IntervalSeconds)
        );

        _timers[endpoint.Id] = timer;
    }

    private async Task CheckEndpointAsync(Endpoint endpoint)
    {
        if (_cts?.Token.IsCancellationRequested == true)
            return;

        try
        {
            var oldResult = _results.TryGetValue(endpoint.Id, out var old) ? old : null;

            var result = endpoint.Type switch
            {
                EndpointType.Http => await _httpMonitor.CheckAsync(endpoint, _cts!.Token),
                EndpointType.Ping => await _pingMonitor.CheckAsync(endpoint, _cts!.Token),
                EndpointType.Tcp => await _tcpMonitor.CheckAsync(endpoint, _cts!.Token),
                _ => throw new NotSupportedException($"Endpoint type {endpoint.Type} not supported")
            };

            _results[endpoint.Id] = result;

            // Notify status change
            if (oldResult != null && oldResult.Status != result.Status)
            {
                _notificationService.NotifyStatusChange(endpoint, oldResult.Status, result.Status);
            }

            ResultUpdated?.Invoke(this, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking endpoint {Name}", endpoint.Name);
        }
    }

    public MonitorResult? GetResult(Guid endpointId)
    {
        return _results.TryGetValue(endpointId, out var result) ? result : null;
    }

    public IEnumerable<MonitorResult> GetAllResults()
    {
        return _results.Values;
    }

    public async Task RefreshAllAsync()
    {
        var tasks = _configService.Settings.Endpoints
            .Where(e => e.IsEnabled)
            .Select(CheckEndpointAsync);

        await Task.WhenAll(tasks);
    }

    public void AddEndpoint(Endpoint endpoint)
    {
        if (endpoint.IsEnabled && _cts?.Token.IsCancellationRequested == false)
        {
            _logger.LogInformation("Adding new endpoint to monitoring: {Name}", endpoint.Name);
            StartMonitoring(endpoint);
        }
    }

    public void RemoveEndpoint(Guid endpointId)
    {
        if (_timers.TryRemove(endpointId, out var timer))
        {
            timer.Dispose();
            _logger.LogInformation("Removed endpoint from monitoring: {Id}", endpointId);
        }

        _results.TryRemove(endpointId, out _);
    }
}
