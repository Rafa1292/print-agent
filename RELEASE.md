# Guía de Release - PrintAgent

## Requisitos previos

### Para release manual:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Inno Setup 6](https://jrsoftware.org/isdl.php)
- [GitHub CLI (gh)](https://cli.github.com/)
- Autenticación en GitHub: `gh auth login`

### Para release automático (GitHub Actions):
- Solo necesitas hacer push de un tag

---

## Opción 1: Release Automático (Recomendado)

Simplemente crea y pushea un tag con el formato `vX.Y.Z`:

```bash
# Crear tag
git tag -a v1.0.0 -m "Release v1.0.0"

# Push del tag
git push origin v1.0.0
```

GitHub Actions automáticamente:
1. Compila el proyecto
2. Crea el instalador con Inno Setup
3. Publica el release con el instalador adjunto

Ver el progreso en: `https://github.com/TU_USUARIO/print-agent/actions`

---

## Opción 2: Release Manual

Usar el script de publicación:

```powershell
# Desde la carpeta del proyecto
.\scripts\publish-release.ps1 -Version "1.0.0"

# Para crear como borrador primero
.\scripts\publish-release.ps1 -Version "1.0.0" -Draft

# Si ya compilaste y solo quieres crear el release
.\scripts\publish-release.ps1 -Version "1.0.0" -SkipBuild
```

---

## URLs de Descarga

Una vez publicado el release, las URLs serán:

```
# Página del release
https://github.com/TU_USUARIO/print-agent/releases/tag/v1.0.0

# Descarga directa del instalador
https://github.com/TU_USUARIO/print-agent/releases/download/v1.0.0/PrintAgent-Setup-1.0.0.exe

# Siempre la última versión
https://github.com/TU_USUARIO/print-agent/releases/latest/download/PrintAgent-Setup-1.0.0.exe
```

Para obtener siempre la última versión sin importar el número:
```
https://github.com/TU_USUARIO/print-agent/releases/latest
```

---

## Versionado

Usamos [Semantic Versioning](https://semver.org/):

- **MAJOR** (1.0.0 → 2.0.0): Cambios incompatibles con versiones anteriores
- **MINOR** (1.0.0 → 1.1.0): Nueva funcionalidad compatible
- **PATCH** (1.0.0 → 1.0.1): Corrección de bugs

---

## Checklist de Release

Antes de crear un release:

- [ ] Todos los cambios están commiteados
- [ ] El proyecto compila sin errores (`dotnet build`)
- [ ] Probado manualmente (servicio + UI)
- [ ] Actualizar CHANGELOG si existe
- [ ] Decidir número de versión según cambios

---

## Troubleshooting

### "gh: command not found"
```powershell
# Instalar GitHub CLI
winget install GitHub.cli
# o
choco install gh
```

### "ISCC.exe not found"
Instalar Inno Setup desde https://jrsoftware.org/isdl.php

### "gh auth: not logged in"
```powershell
gh auth login
# Seguir las instrucciones para autenticarse
```

### El release no aparece
Verificar en Actions que el workflow completó correctamente:
`https://github.com/TU_USUARIO/print-agent/actions`
