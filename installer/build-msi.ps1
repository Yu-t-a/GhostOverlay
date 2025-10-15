# PowerShell script to create MSI installer for GhostOverlay
# This script uses the WiX Toolset to create a Windows Installer (.msi) file

Write-Host "=== GhostOverlay MSI Builder ===" -ForegroundColor Cyan

# Check if WiX is installed
$wixPath = "C:\Program Files (x86)\WiX Toolset v3.14\bin"
if (-not (Test-Path $wixPath)) {
    Write-Host "ERROR: WiX Toolset not found at $wixPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install WiX Toolset v3.14 from:" -ForegroundColor Yellow
    Write-Host "https://github.com/wixtoolset/wix3/releases" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Alternative: Use 'dotnet tool install --global wix'" -ForegroundColor Yellow
    exit 1
}

# Set paths
$candle = Join-Path $wixPath "candle.exe"
$light = Join-Path $wixPath "light.exe"
$projectRoot = Split-Path $PSScriptRoot -Parent
$publishDir = Join-Path $projectRoot "publish"
$installerDir = $PSScriptRoot
$wixFile = Join-Path $installerDir "GhostOverlay.wxs"
$wixObj = Join-Path $installerDir "GhostOverlay.wixobj"
$msiFile = Join-Path $installerDir "GhostOverlay-Setup.msi"

Write-Host "Project root: $projectRoot" -ForegroundColor Gray
Write-Host "Publish dir: $publishDir" -ForegroundColor Gray
Write-Host "Installer dir: $installerDir" -ForegroundColor Gray

# Step 1: Compile WiX source
Write-Host ""
Write-Host "Step 1: Compiling WiX source..." -ForegroundColor Cyan
& $candle -nologo -arch x64 -ext WixUIExtension -out $wixObj $wixFile

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to compile WiX source" -ForegroundColor Red
    exit 1
}

# Step 2: Link and create MSI
Write-Host ""
Write-Host "Step 2: Creating MSI installer..." -ForegroundColor Cyan
& $light -nologo -ext WixUIExtension -cultures:en-us -out $msiFile $wixObj

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create MSI" -ForegroundColor Red
    exit 1
}

# Clean up
Remove-Item $wixObj -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "SUCCESS! MSI installer created:" -ForegroundColor Green
Write-Host $msiFile -ForegroundColor White
Write-Host ""
