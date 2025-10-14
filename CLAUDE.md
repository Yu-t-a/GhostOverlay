# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GhostOverlay is a Windows desktop application (.NET 8.0 WPF) that provides real-time server and website monitoring through a transparent, always-on-top overlay window. It supports HTTP/HTTPS, ICMP Ping, and TCP port monitoring with color-coded status indicators and desktop notifications.

## Build and Run Commands

```bash
# Restore dependencies
dotnet restore

# Build the project (Debug)
dotnet build

# Build for Release
dotnet build -c Release

# Run the application
dotnet run

# Clean build artifacts
dotnet clean
```

## Architecture Overview

### Core Application Structure

The application follows the **MVVM (Model-View-ViewModel)** pattern with **Dependency Injection** using Microsoft.Extensions.Hosting. The entry point is `App.xaml.cs:23-87`, which configures services, creates the overlay window, and initializes the monitoring system.

**Startup Flow:**
1. `App.OnStartup()` sets up Serilog logging to `%LocalAppData%\GhostOverlay\logs\`
2. Configures DI container with all services and ViewModels
3. Creates `OverlayWindow` and injects `OverlayViewModel`
4. Initializes `HotkeyHelper` for global hotkey (Ctrl+Shift+O by default)
5. Sets up `TrayIconHelper` for system tray functionality
6. Starts `MonitoringService` which begins monitoring all enabled endpoints

### Service Layer Architecture

**MonitoringService** (`Services/MonitoringService.cs`) is the orchestrator:
- Maintains a `ConcurrentDictionary<Guid, MonitorResult>` for current results
- Creates a `Timer` per endpoint for periodic checks (see `StartMonitoring:63-77`)
- Routes endpoint checks to appropriate monitor service via `CheckEndpointAsync:79-110`:
  - `HttpMonitorService` - Uses HEAD requests, validates SSL by default
  - `PingMonitorService` - ICMP ping monitoring
  - `TcpMonitorService` - TCP port connectivity checks
- Fires `ResultUpdated` event that `OverlayViewModel` subscribes to for UI updates
- Handles endpoint lifecycle: add, remove, refresh operations

**IMonitorService Interface** (`Services/IMonitorService.cs`):
All monitor services implement this single-method interface:
```csharp
Task<MonitorResult> CheckAsync(Endpoint endpoint, CancellationToken ct = default);
```

**ConfigService** (`Services/ConfigService.cs`):
- Manages application settings at `%AppData%\GhostOverlay\appsettings.json`
- Handles endpoint CRUD operations
- Supports import/export of endpoint configurations
- Creates default endpoints (Google, GitHub) on first run

**NotificationService** (`Services/NotificationService.cs`):
- Integrates with Windows system tray icon for toast notifications
- Notifies on endpoint status changes (Up → Down, Slow, etc.)

### Data Models

**Endpoint** (`Models/Endpoint.cs`):
- Core monitoring configuration (Name, Target, Type, Interval, Timeout, SlowThreshold)
- `EndpointType` enum: Http (0), Ping (1), Tcp (2)
- Supports retry configuration with exponential backoff

**MonitorResult** (`Models/MonitorResult.cs`):
- Stores check results (Status, LatencyMs, LastChecked, ErrorMessage, HttpStatusCode)
- `EndpointStatus` enum: Up (Green), Slow (Yellow), Down (Red), Unknown (Gray)

**AppSettings** (`Models/AppSettings.cs`):
- Global application configuration
- Hotkey configuration (Modifiers, Key)
- Theme, notification preferences, startup behavior
- List of all endpoint configurations

### MVVM Pattern

**OverlayViewModel** (`ViewModels/OverlayViewModel.cs:10-126`):
- Manages `ObservableCollection<EndpointResultViewModel>` bound to UI
- Subscribes to `MonitoringService.ResultUpdated` event (line 27)
- Updates UI via `Application.Current.Dispatcher.Invoke()` for thread safety
- Provides `RefreshCommand` for manual refresh operations
- Calculates status summary (X Up, Y Slow, Z Down)

**EndpointResultViewModel** (`ViewModels/EndpointResultViewModel.cs`):
- Wraps display data for a single endpoint result
- Implements `INotifyPropertyChanged` via `ViewModelBase`

**ViewModelBase** (`ViewModels/ViewModelBase.cs`):
- Base class implementing `INotifyPropertyChanged`
- Provides `SetProperty()` helper for property change notifications

### WPF Views

**OverlayWindow** (`Views/OverlayWindow.xaml`):
- Transparent, topmost window with click-through toggle
- Displays real-time endpoint monitoring status
- Draggable via left-click

**SettingsWindow** (`Views/SettingsWindow.xaml`):
- Endpoint management (add, edit, delete)
- Application settings (hotkey, theme, notifications)
- Import/export functionality

**AddMonitorWindow** (`Views/AddMonitorWindow.xaml`):
- Dialog for creating/editing endpoints
- Validates configuration before saving

### Helpers

**HotkeyHelper** (`Helpers/HotkeyHelper.cs`):
- Uses Win32 `RegisterHotKey` API via P/Invoke
- Registers global hotkey for showing/hiding overlay
- Hooks into WPF window message loop via `HwndSource`

**TrayIconHelper** (`Helpers/TrayIconHelper.cs`):
- Manages Windows Forms `NotifyIcon` for system tray
- Provides context menu for quick actions
- Integrates with notification system

**Value Converters** (`Helpers/`):
- `StatusToColorConverter` - Maps `EndpointStatus` to Brush colors
- `BoolToVisibilityConverter` - Standard WPF converter

### Theming

**DarkTheme.xaml** (`Resources/Styles/DarkTheme.xaml`):
- Global application theme styles
- Color scheme for overlay and settings windows
- Thai language font support (Leelawadee UI) configured in `App.xaml:6-31`

## Configuration File

Settings stored as JSON at: `%AppData%\GhostOverlay\appsettings.json`

**Hotkey Values:**
- `HotkeyModifiers`: Bitmask (Control=2, Shift=4, Alt=1, Win=8)
- `HotkeyKey`: Virtual key code (e.g., O=79)
- Default: Ctrl+Shift+O = Modifiers:5 (Control|Shift), Key:79

## Key Implementation Notes

1. **Thread Safety**: All UI updates from background monitoring threads use `Application.Current.Dispatcher.Invoke()`

2. **Timer Management**: Each enabled endpoint gets its own `System.Threading.Timer` that fires every `IntervalSeconds`. Timers are tracked in `MonitoringService._timers` dictionary.

3. **Status Determination Logic** (`HttpMonitorService.cs:44-59`):
   - 2xx + latency ≤ threshold → Up (Green)
   - 2xx + latency > threshold → Slow (Yellow)
   - 5xx → Down (Red)
   - 4xx → Slow (Yellow) with warning
   - Timeout/Exception → Down (Red)

4. **Exponential Backoff**: Configured per endpoint via `RetryCount` and `UseExponentialBackoff` properties (implementation in individual monitor services).

5. **SSL Validation**: `HttpMonitorService` validates SSL certificates by default. To disable for testing, configure `HttpClient` handler in `App.xaml.cs:46`.

6. **Logging**: Serilog writes to `%LocalAppData%\GhostOverlay\logs\app-YYYYMMDD.log` with 7-day retention.

## Development Workflow

When adding a new endpoint type:
1. Implement `IMonitorService` interface
2. Register service in `App.xaml.cs` DI configuration
3. Add enum value to `EndpointType` in `Models/Endpoint.cs`
4. Update switch statement in `MonitoringService.CheckEndpointAsync:88-94`
5. Add UI support in `AddMonitorWindow.xaml`

When modifying UI:
- Update XAML view
- Ensure ViewModel implements `INotifyPropertyChanged` properly
- Use `ObservableCollection` for lists
- Test with different themes (Dark/Light)

## Windows-Specific Features

- **Always on Top**: Window.Topmost property
- **Click-through**: Extended window styles via Win32 interop
- **Global Hotkeys**: RegisterHotKey Win32 API
- **System Tray**: Windows Forms NotifyIcon component
- **Desktop Notifications**: Windows 10/11 toast notifications via NotifyIcon

## Testing Endpoints

Test endpoint configurations:
- HTTP: `https://www.google.com` (should return 200)
- Ping: `8.8.8.8` (Google DNS)
- TCP: `google.com:443` (HTTPS port)
