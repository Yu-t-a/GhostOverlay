namespace GhostOverlay.Models;

public class Endpoint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty; // URL/IP/Domain
    public EndpointType Type { get; set; } = EndpointType.Http;
    public bool IsEnabled { get; set; } = true;
    public int TimeoutMs { get; set; } = 5000;
    public int IntervalSeconds { get; set; } = 60;
    public int SlowThresholdMs { get; set; } = 800;
    public int RetryCount { get; set; } = 3;
    public bool UseExponentialBackoff { get; set; } = true;
}

public enum EndpointType
{
    Http,
    Ping,
    Tcp
}
