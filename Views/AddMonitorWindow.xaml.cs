using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;
using GhostOverlay.Models;
using MessageBox = System.Windows.MessageBox;

namespace GhostOverlay.Views;

public partial class AddMonitorWindow : Window
{
    private Endpoint? _editingEndpoint;

    public Endpoint? Result { get; private set; }

    public AddMonitorWindow()
    {
        InitializeComponent();

        // Initialize UI after components are loaded
        if (TypeComboBox != null && TypeComboBox.Items.Count > 0)
        {
            TypeComboBox.SelectedIndex = 0; // Select first item (HTTP/HTTPS)
        }

        UpdateTargetHint();
        UpdateTypeDescription();
    }

    // Constructor for editing existing endpoint
    public AddMonitorWindow(Endpoint endpoint) : this()
    {
        _editingEndpoint = endpoint;
        Title = "Edit Monitor";

        // Populate fields with existing endpoint data
        NameTextBox.Text = endpoint.Name;
        TargetTextBox.Text = endpoint.Target;

        // Select the correct type
        TypeComboBox.SelectedIndex = endpoint.Type switch
        {
            EndpointType.Http => 0,
            EndpointType.Ping => 1,
            EndpointType.Tcp => 2,
            _ => 0
        };

        IsEnabledCheckBox.IsChecked = endpoint.IsEnabled;
        IntervalTextBox.Text = endpoint.IntervalSeconds.ToString();
        TimeoutTextBox.Text = endpoint.TimeoutMs.ToString();
        SlowThresholdTextBox.Text = endpoint.SlowThresholdMs.ToString();
        RetryCountTextBox.Text = endpoint.RetryCount.ToString();
        ExponentialBackoffCheckBox.IsChecked = endpoint.UseExponentialBackoff;

        // Change button text
        if (AddButton != null)
        {
            AddButton.Content = "💾 Save Changes";
        }
    }

    private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateTargetHint();
        UpdateTypeDescription();
    }

    private void UpdateTargetHint()
    {
        if (TypeComboBox?.SelectedItem is not ComboBoxItem selectedItem)
            return;

        if (TargetHintText == null)
            return;

        var type = selectedItem.Tag?.ToString();

        TargetHintText.Text = type switch
        {
            "Http" => "Example: https://www.google.com or http://192.168.1.1:8080",
            "Ping" => "Example: 8.8.8.8 or google.com",
            "Tcp" => "Example: 192.168.1.1:3306 or google.com:443",
            _ => ""
        };
    }

    private void UpdateTypeDescription()
    {
        if (TypeComboBox?.SelectedItem is not ComboBoxItem selectedItem)
            return;

        if (TypeDescriptionText == null)
            return;

        var type = selectedItem.Tag?.ToString();

        TypeDescriptionText.Text = type switch
        {
            "Http" => "Monitors HTTP/HTTPS endpoints by sending HEAD requests. Supports SSL certificate validation and various HTTP status codes.",
            "Ping" => "Uses ICMP ping to check if a host is reachable. Works with IP addresses and domain names. May require administrator privileges.",
            "Tcp" => "Tests TCP port connectivity. Useful for checking databases, APIs, or any TCP service. Requires host:port format.",
            _ => ""
        };
    }

    private async void Test_Click(object sender, RoutedEventArgs e)
    {
        // Validate inputs
        if (!ValidateInputs(out var errorMessage))
        {
            MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Disable button during test
        TestButton.IsEnabled = false;
        TestButton.Content = "⏳ Testing...";

        try
        {
            var endpoint = CreateEndpointFromInputs();
            var sw = Stopwatch.StartNew();

            (bool success, string resultMessage) = endpoint.Type switch
            {
                EndpointType.Http => await TestHttpAsync(endpoint.Target),
                EndpointType.Ping => await TestPingAsync(endpoint.Target),
                EndpointType.Tcp => await TestTcpAsync(endpoint.Target),
                _ => (false, "Unknown endpoint type")
            };

            sw.Stop();

            var icon = success ? MessageBoxImage.Information : MessageBoxImage.Warning;
            var title = success ? "Connection Test - Success" : "Connection Test - Failed";
            var message = $"{resultMessage}\n\nLatency: {sw.ElapsedMilliseconds} ms";

            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Test failed with error:\n\n{ex.Message}",
                "Connection Test - Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
        finally
        {
            TestButton.IsEnabled = true;
            TestButton.Content = "🔍 Test Connection";
        }
    }

    private async Task<(bool success, string message)> TestHttpAsync(string url)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var response = await httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, url),
                HttpCompletionOption.ResponseHeadersRead
            );

            var message = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                         $"Target: {url}\n" +
                         $"Status: {(response.IsSuccessStatusCode ? "✓ Success" : "⚠ Warning")}";

            return (response.IsSuccessStatusCode, message);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"HTTP request failed:\n{ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return (false, "Connection timed out after 10 seconds");
        }
    }

    private async Task<(bool success, string message)> TestPingAsync(string host)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(host, 5000);

            if (reply.Status == IPStatus.Success)
            {
                var message = $"Ping successful!\n" +
                             $"Target: {host}\n" +
                             $"Round-trip time: {reply.RoundtripTime} ms\n" +
                             $"TTL: {reply.Options?.Ttl}";
                return (true, message);
            }
            else
            {
                var message = $"Ping failed:\n" +
                             $"Target: {host}\n" +
                             $"Status: {reply.Status}";
                return (false, message);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Ping failed:\n{ex.Message}");
        }
    }

    private async Task<(bool success, string message)> TestTcpAsync(string target)
    {
        try
        {
            var parts = target.Split(':');
            if (parts.Length != 2)
            {
                return (false, "Invalid format. Use host:port (e.g., google.com:443)");
            }

            var host = parts[0];
            if (!int.TryParse(parts[1], out var port))
            {
                return (false, "Invalid port number");
            }

            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port).WaitAsync(TimeSpan.FromSeconds(5));

            var message = $"TCP connection successful!\n" +
                         $"Target: {host}:{port}\n" +
                         $"Status: ✓ Port is open";
            return (true, message);
        }
        catch (SocketException ex)
        {
            return (false, $"TCP connection failed:\n{ex.Message}");
        }
        catch (TimeoutException)
        {
            return (false, "Connection timed out after 5 seconds");
        }
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInputs(out var errorMessage))
        {
            MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Result = CreateEndpointFromInputs();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Header_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        {
            try
            {
                this.DragMove();
            }
            catch
            {
                // DragMove can throw if window is not being dragged properly
            }
        }
    }

    private bool ValidateInputs(out string errorMessage)
    {
        errorMessage = string.Empty;

        // Validate Name
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            errorMessage = "Please enter a monitor name.";
            NameTextBox.Focus();
            return false;
        }

        // Validate Target
        if (string.IsNullOrWhiteSpace(TargetTextBox.Text))
        {
            errorMessage = "Please enter a target URL, IP, or hostname.";
            TargetTextBox.Focus();
            return false;
        }

        // Validate type-specific format
        var selectedItem = TypeComboBox.SelectedItem as ComboBoxItem;
        var type = selectedItem?.Tag?.ToString();

        switch (type)
        {
            case "Http":
                if (!TargetTextBox.Text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !TargetTextBox.Text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = "HTTP monitor requires URL starting with http:// or https://";
                    TargetTextBox.Focus();
                    return false;
                }
                break;

            case "Tcp":
                var parts = TargetTextBox.Text.Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[1], out var port) || port < 1 || port > 65535)
                {
                    errorMessage = "TCP monitor requires format: host:port (e.g., 192.168.1.1:3306)";
                    TargetTextBox.Focus();
                    return false;
                }
                break;
        }

        // Validate numeric inputs
        if (!int.TryParse(IntervalTextBox.Text, out var interval) || interval < 5)
        {
            errorMessage = "Check interval must be at least 5 seconds.";
            IntervalTextBox.Focus();
            return false;
        }

        if (!int.TryParse(TimeoutTextBox.Text, out var timeout) || timeout < 100)
        {
            errorMessage = "Timeout must be at least 100 milliseconds.";
            TimeoutTextBox.Focus();
            return false;
        }

        if (!int.TryParse(SlowThresholdTextBox.Text, out var slowThreshold) || slowThreshold < 50)
        {
            errorMessage = "Slow threshold must be at least 50 milliseconds.";
            SlowThresholdTextBox.Focus();
            return false;
        }

        if (!int.TryParse(RetryCountTextBox.Text, out var retryCount) || retryCount < 0 || retryCount > 10)
        {
            errorMessage = "Retry count must be between 0 and 10.";
            RetryCountTextBox.Focus();
            return false;
        }

        return true;
    }

    private Endpoint CreateEndpointFromInputs()
    {
        var selectedItem = TypeComboBox.SelectedItem as ComboBoxItem;
        var typeString = selectedItem?.Tag?.ToString() ?? "Http";

        var type = typeString switch
        {
            "Http" => EndpointType.Http,
            "Ping" => EndpointType.Ping,
            "Tcp" => EndpointType.Tcp,
            _ => EndpointType.Http
        };

        return new Endpoint
        {
            Id = _editingEndpoint?.Id ?? Guid.NewGuid(), // Keep existing ID when editing
            Name = NameTextBox.Text.Trim(),
            Target = TargetTextBox.Text.Trim(),
            Type = type,
            IsEnabled = IsEnabledCheckBox.IsChecked ?? true,
            IntervalSeconds = int.Parse(IntervalTextBox.Text),
            TimeoutMs = int.Parse(TimeoutTextBox.Text),
            SlowThresholdMs = int.Parse(SlowThresholdTextBox.Text),
            RetryCount = int.Parse(RetryCountTextBox.Text),
            UseExponentialBackoff = ExponentialBackoffCheckBox.IsChecked ?? true
        };
    }
}
