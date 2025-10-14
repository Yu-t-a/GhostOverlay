using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using GhostOverlay.Models;

namespace GhostOverlay.Services;

public class ConfigService
{
    private readonly string _configPath;
    private AppSettings _settings = null!;

    public ConfigService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "GhostOverlay");
        Directory.CreateDirectory(appFolder);

        _configPath = Path.Combine(appFolder, "appsettings.json");
        LoadSettings();
    }

    public AppSettings Settings => _settings;

    private void LoadSettings()
    {
        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? CreateDefaultSettings();
            }
            catch
            {
                _settings = CreateDefaultSettings();
                SaveSettings();
            }
        }
        else
        {
            _settings = CreateDefaultSettings();
            SaveSettings();
        }
    }

    public void SaveSettings()
    {
        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_configPath, json);
    }

    private AppSettings CreateDefaultSettings()
    {
        return new AppSettings
        {
            HotkeyModifiers = ModifierKeys.Control | ModifierKeys.Shift,
            HotkeyKey = Key.O,
            PollIntervalSeconds = 60,
            SlowThresholdMs = 800,
            Theme = "Dark",
            Endpoints = new List<Endpoint>
            {
                new Endpoint
                {
                    Id = Guid.NewGuid(),
                    Name = "Google",
                    Target = "https://www.google.com",
                    Type = EndpointType.Http,
                    IsEnabled = true,
                    IntervalSeconds = 60,
                    TimeoutMs = 5000,
                    SlowThresholdMs = 800
                },
                new Endpoint
                {
                    Id = Guid.NewGuid(),
                    Name = "GitHub",
                    Target = "https://www.github.com",
                    Type = EndpointType.Http,
                    IsEnabled = true,
                    IntervalSeconds = 60,
                    TimeoutMs = 5000,
                    SlowThresholdMs = 800
                }
            }
        };
    }

    public void ExportEndpoints(string filePath)
    {
        var json = JsonSerializer.Serialize(_settings.Endpoints, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(filePath, json);
    }

    public void ImportEndpoints(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var endpoints = JsonSerializer.Deserialize<List<Endpoint>>(json);
        if (endpoints != null)
        {
            _settings.Endpoints = endpoints;
            SaveSettings();
        }
    }

    public void AddEndpoint(Endpoint endpoint)
    {
        _settings.Endpoints.Add(endpoint);
        SaveSettings();
    }

    public void UpdateEndpoint(Endpoint endpoint)
    {
        var existing = _settings.Endpoints.FirstOrDefault(e => e.Id == endpoint.Id);
        if (existing != null)
        {
            var index = _settings.Endpoints.IndexOf(existing);
            _settings.Endpoints[index] = endpoint;
            SaveSettings();
        }
    }

    public void RemoveEndpoint(Guid endpointId)
    {
        var endpoint = _settings.Endpoints.FirstOrDefault(e => e.Id == endpointId);
        if (endpoint != null)
        {
            _settings.Endpoints.Remove(endpoint);
            SaveSettings();
        }
    }
}
