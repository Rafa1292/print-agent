# Install PrintAgent as Windows Service
# Run as Administrator

$ErrorActionPreference = "Stop"
$ServiceName = "PrintAgent.Service"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Split-Path -Parent $ScriptDir
$ServicePath = Join-Path $ProjectDir "publish\service\PrintAgent.Service.exe"

# Check if running as admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "Error: Este script debe ejecutarse como Administrador" -ForegroundColor Red
    exit 1
}

# Check if service exe exists
if (-not (Test-Path $ServicePath)) {
    Write-Host "Error: No se encontró el ejecutable del servicio" -ForegroundColor Red
    Write-Host "Ejecute primero: .\build.ps1 -Release" -ForegroundColor Yellow
    exit 1
}

# Check if service already exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($existingService) {
    Write-Host "Servicio ya existe. Deteniendo..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2

    Write-Host "Eliminando servicio existente..." -ForegroundColor Yellow
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 1
}

# Create the service
Write-Host "Creando servicio Windows..." -ForegroundColor Cyan
sc.exe create $ServiceName binPath= "`"$ServicePath`"" start= auto DisplayName= "PrintAgent Service"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al crear el servicio" -ForegroundColor Red
    exit 1
}

# Set description
sc.exe description $ServiceName "Servicio de impresión para aplicaciones web"

# Start the service
Write-Host "Iniciando servicio..." -ForegroundColor Cyan
Start-Service -Name $ServiceName

# Verify
$service = Get-Service -Name $ServiceName
if ($service.Status -eq "Running") {
    Write-Host ""
    Write-Host "Servicio instalado e iniciado correctamente!" -ForegroundColor Green
    Write-Host "URL: http://localhost:5123" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Para verificar: Invoke-RestMethod http://localhost:5123/health" -ForegroundColor Yellow
}
else {
    Write-Host "Error: El servicio no se inició correctamente" -ForegroundColor Red
    Write-Host "Estado: $($service.Status)" -ForegroundColor Red
}
