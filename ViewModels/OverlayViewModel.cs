using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GhostOverlay.Models;
using GhostOverlay.Services;
using Application = System.Windows.Application;

namespace GhostOverlay.ViewModels;

public class OverlayViewModel : ViewModelBase
{
    private readonly ConfigService _configService;
    private readonly MonitoringService _monitoringService;
    private bool _isClickThrough;
    private string _statusSummary = string.Empty;

    public OverlayViewModel(ConfigService configService, MonitoringService monitoringService)
    {
        _configService = configService;
        _monitoringService = monitoringService;

        MonitorResults = new ObservableCollection<EndpointResultViewModel>();

        RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
        EditCommand = new RelayCommand(EditEndpoint);
        DeleteCommand = new RelayCommand(DeleteEndpoint);

        // Subscribe to result updates
        _monitoringService.ResultUpdated += OnResultUpdated;

        // Initial load
        LoadEndpoints();
    }

    public ObservableCollection<EndpointResultViewModel> MonitorResults { get; }

    public bool IsClickThrough
    {
        get => _isClickThrough;
        set => SetProperty(ref _isClickThrough, value);
    }

    public string StatusSummary
    {
        get => _statusSummary;
        set => SetProperty(ref _statusSummary, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    private void LoadEndpoints()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MonitorResults.Clear();

            foreach (var endpoint in _configService.Settings.Endpoints.Where(e => e.IsEnabled))
            {
                var vm = new EndpointResultViewModel();
                var result = _monitoringService.GetResult(endpoint.Id);

                if (result != null)
                {
                    vm.UpdateFromResult(result, endpoint);
                }
                else
                {
                    vm.EndpointName = endpoint.Name;
                    vm.Target = endpoint.Target;
                    vm.Status = EndpointStatus.Unknown;
                }

                MonitorResults.Add(vm);
            }

            UpdateStatusSummary();
        });
    }

    private void OnResultUpdated(object? sender, MonitorResult result)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var endpoint = _configService.Settings.Endpoints.FirstOrDefault(e => e.Id == result.EndpointId);
            if (endpoint == null) return;

            var vm = MonitorResults.FirstOrDefault(m => m.Target == endpoint.Target);
            if (vm != null)
            {
                vm.UpdateFromResult(result, endpoint);
            }
            else
            {
                vm = new EndpointResultViewModel();
                vm.UpdateFromResult(result, endpoint);
                MonitorResults.Add(vm);
            }

            UpdateStatusSummary();
        });
    }

    private async Task RefreshAsync()
    {
        await _monitoringService.RefreshAllAsync();
    }

    private void UpdateStatusSummary()
    {
        var upCount = MonitorResults.Count(m => m.Status == EndpointStatus.Up);
        var slowCount = MonitorResults.Count(m => m.Status == EndpointStatus.Slow);
        var downCount = MonitorResults.Count(m => m.Status == EndpointStatus.Down);

        StatusSummary = $"{upCount} Up, {slowCount} Slow, {downCount} Down";
    }

    public void AddEndpoint(Endpoint endpoint)
    {
        // Add to config
        _configService.AddEndpoint(endpoint);

        // Add to monitoring service
        _monitoringService.AddEndpoint(endpoint);

        // Reload UI
        LoadEndpoints();
    }

    private void EditEndpoint(object? parameter)
    {
        if (parameter is not EndpointResultViewModel vm) return;

        // Find the endpoint
        var endpoint = _configService.Settings.Endpoints.FirstOrDefault(e => e.Id == vm.EndpointId);
        if (endpoint == null) return;

        // Open edit window
        var editWindow = new Views.AddMonitorWindow(endpoint);
        if (editWindow.ShowDialog() == true && editWindow.Result != null)
        {
            // Update endpoint in config
            _configService.UpdateEndpoint(editWindow.Result);

            // Restart monitoring for this endpoint
            _monitoringService.RemoveEndpoint(vm.EndpointId);
            _monitoringService.AddEndpoint(editWindow.Result);

            // Reload UI
            LoadEndpoints();
        }
    }

    private void DeleteEndpoint(object? parameter)
    {
        if (parameter is not EndpointResultViewModel vm) return;

        // Confirm deletion
        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete '{vm.EndpointName}'?",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        // Remove from monitoring service
        _monitoringService.RemoveEndpoint(vm.EndpointId);

        // Remove from config
        _configService.RemoveEndpoint(vm.EndpointId);

        // Reload UI
        LoadEndpoints();
    }
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);
}
