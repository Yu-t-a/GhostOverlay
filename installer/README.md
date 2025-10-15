# GhostOverlay Installer

This directory contains the files needed to create a Windows Installer (.msi) for GhostOverlay.

## Prerequisites

To build the MSI installer, you need to install **WiX Toolset v3.14**:

### Option 1: Download WiX Toolset (Recommended)
1. Download from: https://github.com/wixtoolset/wix3/releases/tag/wix3141rtm
2. Install the `.exe` file
3. Default installation path: `C:\Program Files (x86)\WiX Toolset v3.14\bin`

### Option 2: Install via .NET CLI
```bash
dotnet tool install --global wix
```

## Building the MSI

### Method 1: Using PowerShell Script (Easy)
```powershell
cd installer
.\build-msi.ps1
```

### Method 2: Manual Build
```powershell
# 1. Set WiX path
$env:WIX = "C:\Program Files (x86)\WiX Toolset v3.14\bin"

# 2. Compile WiX source
& "$env:WIX\candle.exe" -nologo -arch x64 -ext WixUIExtension GhostOverlay.wxs

# 3. Link and create MSI
& "$env:WIX\light.exe" -nologo -ext WixUIExtension -cultures:en-us -out GhostOverlay-Setup.msi GhostOverlay.wixobj
```

## Output

After building, you will get:
- `GhostOverlay-Setup.msi` - Windows Installer package

## What the Installer Does

1. **Installs to:** `C:\Program Files\GhostOverlay\`
2. **Creates shortcuts:**
   - Start Menu: `GhostOverlay`
   - Desktop: `GhostOverlay` (optional)
3. **Registers in:** Add/Remove Programs
4. **Supports:** Clean uninstallation

## MSI Features

- ✅ Per-machine installation
- ✅ Major upgrade support (auto-uninstalls old versions)
- ✅ Start Menu shortcut
- ✅ Desktop shortcut
- ✅ Add/Remove Programs integration
- ✅ Clean uninstallation
- ✅ License agreement dialog

## Files

- `GhostOverlay.wxs` - WiX source file (installer definition)
- `License.rtf` - License agreement shown during installation
- `build-msi.ps1` - PowerShell build script
- `README.md` - This file

## Version Management

To create a new version:
1. Update version in `GhostOverlay.wxs` (line 8)
2. Keep the same `UpgradeCode` (never change this)
3. Rebuild the MSI

## Troubleshooting

### "WiX Toolset not found"
- Install WiX Toolset v3.14 from the link above
- Verify installation: `dir "C:\Program Files (x86)\WiX Toolset v3.14\bin"`

### "candle.exe is not recognized"
- Add WiX to PATH: `$env:PATH += ";C:\Program Files (x86)\WiX Toolset v3.14\bin"`
- Or use the full path in commands

### "Source file not found"
- Make sure you've published the app first: `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish`
