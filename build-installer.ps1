# GhostOverlay Build Script
# สคริปต์นี้จะสร้างทั้ง .exe และ .msi installer

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

Write-Host "=== GhostOverlay Build Script ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Clean previous builds
Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean -c $Configuration
if (Test-Path "publish") {
    Remove-Item -Recurse -Force "publish"
}
Write-Host "✓ Clean completed" -ForegroundColor Green
Write-Host ""

# Step 2: Build self-contained .exe
Write-Host "[2/4] Building self-contained .exe..." -ForegroundColor Yellow
dotnet publish `
    -c $Configuration `
    -r $Runtime `
    --self-contained `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o "publish/$Runtime"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ .exe build completed" -ForegroundColor Green
    $exePath = "publish\$Runtime\GhostOverlay.exe"
    if (Test-Path $exePath) {
        $exeSize = (Get-Item $exePath).Length / 1MB
        Write-Host "  Location: $exePath" -ForegroundColor Cyan
        Write-Host "  Size: $([math]::Round($exeSize, 2)) MB" -ForegroundColor Cyan
    }
} else {
    Write-Host "✗ .exe build failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 3: Check for WiX Toolset
Write-Host "[3/4] Checking for WiX Toolset..." -ForegroundColor Yellow
$wixInstalled = $false

# Check if WiX v5 is installed
try {
    $wixVersion = dotnet tool list --global | Select-String "wix"
    if ($wixVersion) {
        Write-Host "✓ WiX Toolset found (dotnet tool)" -ForegroundColor Green
        $wixInstalled = $true
    }
} catch {
    # WiX not installed as global tool
}

# Check for WiX v4/v5 in PATH
if (-not $wixInstalled) {
    try {
        $wixCheck = Get-Command "wix.exe" -ErrorAction SilentlyContinue
        if ($wixCheck) {
            Write-Host "✓ WiX Toolset found in PATH" -ForegroundColor Green
            $wixInstalled = $true
        }
    } catch {
        # Continue
    }
}

if (-not $wixInstalled) {
    Write-Host "! WiX Toolset not found" -ForegroundColor Yellow
    Write-Host "  To install WiX v5 (recommended):" -ForegroundColor Cyan
    Write-Host "  dotnet tool install --global wix" -ForegroundColor White
    Write-Host ""
    Write-Host "  Or download from: https://wixtoolset.org/releases/" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "✓ .exe is ready at: publish\$Runtime\GhostOverlay.exe" -ForegroundColor Green
    Write-Host "  Skipping .msi creation..." -ForegroundColor Yellow
    exit 0
}
Write-Host ""

# Step 4: Build .msi installer
Write-Host "[4/4] Building .msi installer..." -ForegroundColor Yellow
Push-Location "installer"
try {
    dotnet build -c $Configuration

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ .msi build completed" -ForegroundColor Green

        # Find the .msi file
        $msiPath = Get-ChildItem -Path "bin\$Configuration" -Filter "*.msi" -Recurse | Select-Object -First 1
        if ($msiPath) {
            $msiSize = $msiPath.Length / 1MB
            Write-Host "  Location: installer\$($msiPath.FullName.Replace((Get-Location).Path + '\', ''))" -ForegroundColor Cyan
            Write-Host "  Size: $([math]::Round($msiSize, 2)) MB" -ForegroundColor Cyan
        }
    } else {
        Write-Host "✗ .msi build failed" -ForegroundColor Red
        Write-Host "  Note: .exe is still available at: publish\$Runtime\GhostOverlay.exe" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Error building .msi: $_" -ForegroundColor Red
    Write-Host "  Note: .exe is still available at: publish\$Runtime\GhostOverlay.exe" -ForegroundColor Yellow
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "=== Build Summary ===" -ForegroundColor Cyan
Write-Host "✓ Standalone .exe: publish\$Runtime\GhostOverlay.exe" -ForegroundColor Green
if ($msiPath) {
    Write-Host "✓ MSI Installer: installer\bin\$Configuration\GhostOverlaySetup.msi" -ForegroundColor Green
}
Write-Host ""
