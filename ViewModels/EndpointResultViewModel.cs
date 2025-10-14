using GhostOverlay.Models;

namespace GhostOverlay.ViewModels;

public class EndpointResultViewModel : ViewModelBase
{
    private string _endpointName = string.Empty;
    private string _target = string.Empty;
    private EndpointStatus _status;
    private int _latencyMs;
    private DateTime _lastChecked;
    private string? _errorMessage;

    public string EndpointName
    {
        get => _endpointName;
        set => SetProperty(ref _endpointName, value);
    }

    public string Target
    {
        get => _target;
        set => SetProperty(ref _target, value);
    }

    public EndpointStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public int LatencyMs
    {
        get => _latencyMs;
        set => SetProperty(ref _latencyMs, value);
    }

    public DateTime LastChecked
    {
        get => _lastChecked;
        set => SetProperty(ref _lastChecked, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            SetProperty(ref _errorMessage, value);
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public void UpdateFromResult(MonitorResult result, Endpoint endpoint)
    {
        EndpointName = endpoint.Name;
        Target = endpoint.Target;
        Status = result.Status;
        LatencyMs = result.LatencyMs;
        LastChecked = result.LastChecked;
        ErrorMessage = result.ErrorMessage;
    }
}
