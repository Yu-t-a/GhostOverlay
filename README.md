# GhostOverlay

<div align="center">

![Ghost Overlay Logo](https://img.shields.io/badge/Ghost-Overlay-purple?style=for-the-badge&logo=windows)

**Real-time Server Monitoring with Transparent Desktop Overlay**

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?style=flat-square&logo=windows)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

</div>

---

## 📖 Overview

**GhostOverlay** is a lightweight Windows desktop application that provides real-time server and website monitoring through an always-on-top transparent overlay window. Perfect for developers, system administrators, and DevOps engineers who need to keep an eye on their services without switching windows.

### ✨ Key Features

- 🎯 **Multi-Protocol Monitoring** - HTTP/HTTPS, ICMP Ping, and TCP port monitoring
- 👻 **Ghost Mode** - Transparent, always-on-top overlay with click-through support
- 🎨 **Dark/Light Themes** - Beautiful UI with customizable themes
- ⚡ **Real-time Status** - Live updates with color-coded indicators (Green/Yellow/Red)
- 🔔 **Smart Notifications** - Desktop notifications for status changes
- ⌨️ **Global Hotkey** - Toggle overlay with Ctrl+Shift+O (customizable)
- 💾 **Configuration Management** - Import/export endpoint configurations
- 🔄 **Auto-retry** - Exponential backoff for failed checks
- 🪟 **System Tray** - Runs in background with tray icon

---

## 🚀 Getting Started

### Prerequisites

- Windows 10/11
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Installation

#### Option 1: Download Release (Recommended)
1. Download the latest release from [Releases](https://github.com/Yu-t-a/GhostOverlay/releases)
2. Extract the ZIP file
3. Run `GhostOverlay.exe`

#### Option 2: Build from Source
```bash
# Clone the repository
git clone https://github.com/Yu-t-a/GhostOverlay.git
cd GhostOverlay

# Restore dependencies
dotnet restore

# Build the project
dotnet build -c Release

# Run the application
dotnet run
```

---

## 📚 Usage

### First Launch

1. The application starts minimized in the system tray
2. Press `Ctrl+Shift+O` to show the overlay
3. Right-click the tray icon to access settings

### Adding Endpoints

1. Open **Settings** from the overlay or tray menu
2. Click **Add Endpoint**
3. Configure the endpoint:
   - **Name**: Friendly display name
   - **Target**: URL, IP address, or hostname:port
   - **Type**: HTTP, Ping, or TCP
   - **Interval**: Check frequency (seconds)
   - **Timeout**: Maximum wait time (milliseconds)
   - **Slow Threshold**: Yellow warning threshold (milliseconds)

### Status Indicators

| Color | Status | Description |
|-------|--------|-------------|
| 🟢 Green | Up | Service is responding normally |
| 🟡 Yellow | Slow | Response time exceeds threshold |
| 🔴 Red | Down | Service is unreachable or error |
| ⚫ Gray | Unknown | Not yet checked or disabled |

### Hotkeys

- `Ctrl+Shift+O` - Toggle overlay visibility (customizable in settings)
- `Left Click + Drag` - Move overlay window
- `Settings Button` - Open configuration window
- `Refresh Button` - Force immediate check

---

## ⚙️ Configuration

### Configuration File Location

Settings are stored in JSON format at:
```
%AppData%\GhostOverlay\appsettings.json
```

### Example Configuration

```json
{
  "HotkeyModifiers": 5,
  "HotkeyKey": 79,
  "PollIntervalSeconds": 60,
  "SlowThresholdMs": 800,
  "Theme": "Dark",
  "ShowNotifications": true,
  "StartMinimized": true,
  "AutoStartMonitoring": true,
  "Endpoints": [
    {
      "Id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "Name": "Google",
      "Target": "https://www.google.com",
      "Type": 0,
      "IsEnabled": true,
      "TimeoutMs": 5000,
      "IntervalSeconds": 60,
      "SlowThresholdMs": 800,
      "RetryCount": 3,
      "UseExponentialBackoff": true
    },
    {
      "Id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "Name": "DNS Server",
      "Target": "8.8.8.8",
      "Type": 1,
      "IsEnabled": true,
      "TimeoutMs": 5000,
      "IntervalSeconds": 60,
      "SlowThresholdMs": 100,
      "RetryCount": 3,
      "UseExponentialBackoff": true
    },
    {
      "Id": "5f71e321-892e-4a43-9e84-2c5f5a3d8f92",
      "Name": "HTTPS Port",
      "Target": "google.com:443",
      "Type": 2,
      "IsEnabled": true,
      "TimeoutMs": 5000,
      "IntervalSeconds": 60,
      "SlowThresholdMs": 200,
      "RetryCount": 3,
      "UseExponentialBackoff": true
    }
  ]
}
```

**Important:** This file contains your monitoring endpoints and is stored locally at `%AppData%\GhostOverlay\appsettings.json`. It is automatically excluded from git commits via .gitignore for privacy.

### Endpoint Types

| Type | Value | Description |
|------|-------|-------------|
| Http | 0 | HTTP/HTTPS endpoint (supports GET/HEAD requests) |
| Ping | 1 | ICMP ping (requires hostname or IP) |
| Tcp | 2 | TCP port check (requires `host:port` format) |

---

## 🏗️ Architecture

### Project Structure

```
GhostOverlay/
├── Models/              # Data models (Endpoint, MonitorResult, AppSettings)
├── Services/            # Business logic (monitoring, configuration)
│   ├── HttpMonitorService.cs
│   ├── PingMonitorService.cs
│   ├── TcpMonitorService.cs
│   ├── ConfigService.cs
│   ├── NotificationService.cs
│   └── MonitoringService.cs
├── ViewModels/          # MVVM ViewModels
├── Views/               # WPF Windows and UserControls
├── Helpers/             # Utilities (hotkeys, tray icon, converters)
└── Resources/           # Themes and styles
```

### Technologies Used

- **.NET 8.0** - Modern cross-platform framework
- **WPF (Windows Presentation Foundation)** - Rich UI framework
- **MVVM Pattern** - Clean separation of concerns
- **Dependency Injection** - Microsoft.Extensions.Hosting
- **Serilog** - Structured logging
- **System.Net.Http** - HTTP monitoring
- **System.Net.NetworkInformation** - Ping monitoring
- **System.Net.Sockets** - TCP monitoring

---

## 🔧 Advanced Features

### SSL/TLS Certificate Validation

By default, the application validates SSL certificates. For development/testing with self-signed certificates, you can modify the `HttpMonitorService` configuration (not recommended for production).

### Exponential Backoff

Failed checks automatically use exponential backoff to reduce load on unresponsive services:
- First retry: 2 seconds
- Second retry: 4 seconds
- Third retry: 8 seconds

### Click-through Mode

Enable "Click-through" in the overlay footer to make the window ignore mouse clicks, allowing you to interact with windows beneath it.

### Import/Export Endpoints

**Export:**
1. Open Settings
2. Click **Export Endpoints**
3. Save JSON file

**Import:**
1. Open Settings
2. Click **Import Endpoints**
3. Select JSON file

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Development Setup

1. Install [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Rider](https://www.jetbrains.com/rider/)
2. Install [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
3. Clone the repository
4. Open `GhostOverlay.sln`
5. Build and run

### Code Style

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable names
- Add XML documentation for public APIs
- Write unit tests for business logic

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🐛 Bug Reports & Feature Requests

Please use the [GitHub Issues](https://github.com/Yu-t-a/GhostOverlay/issues) page to report bugs or request features.

---

## 📊 Roadmap

- [ ] Multiple overlay windows
- [ ] Custom themes editor
- [ ] Latency history graphs
- [ ] Webhook notifications (Slack, Discord, Teams)
- [ ] SSL certificate expiry alerts
- [ ] Database history tracking (SQLite)
- [ ] Export monitoring reports
- [ ] Auto-updater

---

## 👨‍💻 Author

Created with ❤️ for the monitoring community

---

## 🙏 Acknowledgments

- Built with [.NET](https://dotnet.microsoft.com/)
- Icons from [Segoe MDL2 Assets](https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font)
- Inspired by system monitoring tools worldwide

---

<div align="center">

**⭐ Star this repository if you find it useful! ⭐**

</div>
