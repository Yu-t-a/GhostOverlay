# Simple MSI builder without WiX Toolset
# This creates a basic installer using built-in Windows tools

Write-Host "=== GhostOverlay Simple Installer Builder ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "NOTE: For a proper .msi installer, you need WiX Toolset." -ForegroundColor Yellow
Write-Host "Download from: https://github.com/wixtoolset/wix3/releases" -ForegroundColor Yellow
Write-Host ""
Write-Host "Alternative: Creating a portable ZIP package instead..." -ForegroundColor Cyan
Write-Host ""

$projectRoot = Split-Path $PSScriptRoot -Parent
$publishDir = Join-Path $projectRoot "publish"
$outputZip = Join-Path $PSScriptRoot "GhostOverlay-Portable.zip"

# Check if publish directory exists
if (-not (Test-Path $publishDir)) {
    Write-Host "ERROR: Publish directory not found: $publishDir" -ForegroundColor Red
    Write-Host "Please run: dotnet publish first" -ForegroundColor Yellow
    exit 1
}

# Create portable ZIP
Write-Host "Creating portable package..." -ForegroundColor Cyan
if (Test-Path $outputZip) {
    Remove-Item $outputZip -Force
}

# Create a README for the portable package
$readmeContent = @"
# GhostOverlay - Portable Edition

## Installation

1. Extract all files to a folder of your choice
2. Run GhostOverlay.exe

## Configuration

Settings are stored in: `%AppData%\GhostOverlay\appsettings.json`

## Uninstallation

Simply delete the application folder.

## Requirements

- Windows 10 or later (64-bit)
- No additional runtime required (self-contained)

## Usage

- **Show/Hide Overlay:** Ctrl+Shift+O (default hotkey)
- **Add Monitor:** Click the ➕ button
- **Settings:** Click the ⚙ button

## Support

For issues and updates, visit:
https://github.com/ghostoverlay/ghostoverlay
"@

$readmePath = Join-Path $publishDir "README.txt"
$readmeContent | Out-File -FilePath $readmePath -Encoding UTF8

# Compress
Compress-Archive -Path "$publishDir\*" -DestinationPath $outputZip -Force

# Clean up
Remove-Item $readmePath -Force

Write-Host ""
Write-Host "SUCCESS! Portable package created:" -ForegroundColor Green
Write-Host $outputZip -ForegroundColor White
Write-Host ""
Write-Host "File size: " -NoNewline
$size = (Get-Item $outputZip).Length / 1MB
Write-Host ("{0:N2} MB" -f $size) -ForegroundColor Cyan
Write-Host ""
