namespace GhostOverlay.Models;

public class MonitorResult
{
    public Guid EndpointId { get; set; }
    public EndpointStatus Status { get; set; } = EndpointStatus.Unknown;
    public int LatencyMs { get; set; }
    public DateTime LastChecked { get; set; }
    public string? ErrorMessage { get; set; }
    public int? HttpStatusCode { get; set; }
}

public enum EndpointStatus
{
    Up,      // Green
    Slow,    // Yellow
    Down,    // Red
    Unknown  // Gray
}
