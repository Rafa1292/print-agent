# Publish PrintAgent Release to GitHub
# Requirements:
#   - GitHub CLI (gh) installed and authenticated
#   - Inno Setup installed (iscc.exe in PATH or default location)
#
# Usage: .\scripts\publish-release.ps1 -Version "1.0.0"

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,

    [switch]$Draft,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Split-Path -Parent $ScriptDir

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  PrintAgent Release Publisher" -ForegroundColor Cyan
Write-Host "  Version: $Version" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check gh CLI
Write-Host "Verificando GitHub CLI..." -ForegroundColor Yellow
$ghVersion = gh --version 2>$null
if (-not $ghVersion) {
    Write-Host "Error: GitHub CLI (gh) no está instalado" -ForegroundColor Red
    Write-Host "Instalar desde: https://cli.github.com/" -ForegroundColor Gray
    exit 1
}

# Check gh auth
$ghAuth = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: GitHub CLI no está autenticado" -ForegroundColor Red
    Write-Host "Ejecutar: gh auth login" -ForegroundColor Gray
    exit 1
}

# Find Inno Setup compiler
$isccPaths = @(
    "iscc.exe",
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe"
)

$iscc = $null
foreach ($path in $isccPaths) {
    if (Get-Command $path -ErrorAction SilentlyContinue) {
        $iscc = $path
        break
    }
    if (Test-Path $path) {
        $iscc = $path
        break
    }
}

if (-not $iscc) {
    Write-Host "Error: Inno Setup (ISCC.exe) no encontrado" -ForegroundColor Red
    Write-Host "Instalar desde: https://jrsoftware.org/isdl.php" -ForegroundColor Gray
    exit 1
}

Write-Host "Inno Setup encontrado: $iscc" -ForegroundColor Gray

# Update version in setup.iss
Write-Host ""
Write-Host "[1/5] Actualizando versión en setup.iss..." -ForegroundColor Yellow
$setupIssPath = Join-Path $ProjectDir "installer\setup.iss"
$setupContent = Get-Content $setupIssPath -Raw
$setupContent = $setupContent -replace '#define MyAppVersion ".*"', "#define MyAppVersion `"$Version`""
Set-Content $setupIssPath $setupContent -NoNewline

# Build project
if (-not $SkipBuild) {
    Write-Host "[2/5] Compilando proyecto..." -ForegroundColor Yellow
    Push-Location $ProjectDir
    try {
        & "$ProjectDir\build.ps1" -Release -Clean
        if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Host "[2/5] Saltando build (usando binarios existentes)..." -ForegroundColor Gray
}

# Create installer
Write-Host "[3/5] Creando instalador..." -ForegroundColor Yellow
$distDir = Join-Path $ProjectDir "dist"
if (-not (Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir | Out-Null
}

& $iscc /Q "$setupIssPath"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al crear instalador" -ForegroundColor Red
    exit 1
}

$installerPath = Join-Path $distDir "PrintAgent-Setup-$Version.exe"
if (-not (Test-Path $installerPath)) {
    Write-Host "Error: Instalador no encontrado en $installerPath" -ForegroundColor Red
    exit 1
}

$installerSize = [math]::Round((Get-Item $installerPath).Length / 1MB, 2)
Write-Host "Instalador creado: $installerPath ($installerSize MB)" -ForegroundColor Gray

# Create git tag
Write-Host "[4/5] Creando tag v$Version..." -ForegroundColor Yellow
Push-Location $ProjectDir
try {
    git tag -a "v$Version" -m "Release v$Version" 2>$null
    if ($LASTEXITCODE -eq 0) {
        git push origin "v$Version"
    }
    else {
        Write-Host "Tag v$Version ya existe, continuando..." -ForegroundColor Gray
    }
}
finally {
    Pop-Location
}

# Create GitHub release
Write-Host "[5/5] Creando release en GitHub..." -ForegroundColor Yellow

$releaseNotes = @"
## PrintAgent v$Version

### Instalador
- **PrintAgent-Setup-$Version.exe** - Instalador completo para Windows

### Cambios
- Ver commits para detalles

### Requisitos
- Windows 10/11 (64-bit)
- No requiere .NET instalado (self-contained)

### Instalación
1. Descargar `PrintAgent-Setup-$Version.exe`
2. Ejecutar como administrador
3. Seguir el asistente de instalación

El servicio se iniciará automáticamente después de la instalación.
"@

$releaseArgs = @(
    "release", "create", "v$Version",
    $installerPath,
    "--title", "PrintAgent v$Version",
    "--notes", $releaseNotes
)

if ($Draft) {
    $releaseArgs += "--draft"
}

Push-Location $ProjectDir
try {
    gh @releaseArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error al crear release" -ForegroundColor Red
        exit 1
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Release v$Version publicado!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

# Get release URL
$releaseUrl = gh release view "v$Version" --json url -q ".url" 2>$null
if ($releaseUrl) {
    Write-Host "URL del release:" -ForegroundColor White
    Write-Host "  $releaseUrl" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "URL de descarga directa:" -ForegroundColor White
    Write-Host "  $releaseUrl/download/PrintAgent-Setup-$Version.exe" -ForegroundColor Cyan
}
