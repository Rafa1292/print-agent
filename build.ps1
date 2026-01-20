# PrintAgent Build Script
# Usage: .\build.ps1

param(
    [switch]$Clean,
    [switch]$Release
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$Configuration = if ($Release) { "Release" } else { "Debug" }
$PublishDir = Join-Path $ScriptDir "publish"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  PrintAgent Build Script" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Clean if requested
if ($Clean) {
    Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Yellow
    if (Test-Path $PublishDir) {
        Remove-Item -Recurse -Force $PublishDir
    }
    dotnet clean "$ScriptDir\PrintAgent.sln" -c $Configuration --nologo -v q
}
else {
    Write-Host "[1/4] Skipping clean (use -Clean to clean)" -ForegroundColor Gray
}

# Restore packages
Write-Host "[2/4] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore "$ScriptDir\PrintAgent.sln" --nologo -v q
if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

# Build solution
Write-Host "[3/4] Building solution..." -ForegroundColor Yellow
dotnet build "$ScriptDir\PrintAgent.sln" -c $Configuration --nologo --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

# Publish both projects
Write-Host "[4/4] Publishing projects..." -ForegroundColor Yellow

# Create publish directories
$ServiceDir = Join-Path $PublishDir "service"
$UIDir = Join-Path $PublishDir "ui"

New-Item -ItemType Directory -Force -Path $ServiceDir | Out-Null
New-Item -ItemType Directory -Force -Path $UIDir | Out-Null

# Publish Service (self-contained for easier deployment)
Write-Host "  - Publishing PrintAgent.Service..." -ForegroundColor Gray
dotnet publish "$ScriptDir\src\PrintAgent.Service\PrintAgent.Service.csproj" `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $ServiceDir `
    --nologo -v q
if ($LASTEXITCODE -ne 0) { throw "Service publish failed" }

# Publish UI (self-contained for easier deployment)
Write-Host "  - Publishing PrintAgent.UI..." -ForegroundColor Gray
dotnet publish "$ScriptDir\src\PrintAgent.UI\PrintAgent.UI.csproj" `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $UIDir `
    --nologo -v q
if ($LASTEXITCODE -ne 0) { throw "UI publish failed" }

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Build completed successfully!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output directories:" -ForegroundColor White
Write-Host "  Service: $ServiceDir" -ForegroundColor Gray
Write-Host "  UI:      $UIDir" -ForegroundColor Gray
Write-Host ""
Write-Host "To create installer, run Inno Setup with installer\setup.iss" -ForegroundColor Yellow
