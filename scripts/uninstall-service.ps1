# Uninstall PrintAgent Windows Service
# Run as Administrator

$ErrorActionPreference = "Stop"
$ServiceName = "PrintAgent.Service"

# Check if running as admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "Error: Este script debe ejecutarse como Administrador" -ForegroundColor Red
    exit 1
}

# Check if service exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if (-not $existingService) {
    Write-Host "El servicio '$ServiceName' no está instalado" -ForegroundColor Yellow
    exit 0
}

# Stop the service
Write-Host "Deteniendo servicio..." -ForegroundColor Cyan
Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Delete the service
Write-Host "Eliminando servicio..." -ForegroundColor Cyan
sc.exe delete $ServiceName | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Servicio desinstalado correctamente!" -ForegroundColor Green
}
else {
    Write-Host "Error al eliminar el servicio" -ForegroundColor Red
    exit 1
}
