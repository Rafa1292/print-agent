# CLAUDE.md - PrintAgent Project

## Descripción

PrintAgent es un servicio de Windows para impresión de tickets en impresoras térmicas ESC/POS. Se comunica con el sistema Nico mediante una API REST local.

## Estructura del Proyecto

```
print-agent/
├── src/
│   └── PrintAgent.Service/
│       ├── Models/          # DTOs y modelos de datos
│       ├── Services/        # Lógica de negocio
│       │   ├── EscPosBuilder.cs    # Constructor de comandos ESC/POS
│       │   ├── TicketBuilder.cs    # Constructor de tickets
│       │   └── PrinterService.cs   # Servicio de impresión
│       └── Program.cs       # Punto de entrada y endpoints API
├── build.ps1               # Script de compilación
├── installer/              # Archivos del instalador (Inno Setup)
└── publish/                # Salida de compilación
```

## Distribución

### IMPORTANTE: Versionado y Releases

El proyecto se distribuye mediante un **instalador** que se genera en GitHub al crear un tag. **Cada vez que se realice algún cambio, se debe generar un nuevo tag con la versión incrementada.**

### Proceso de Release

1. **Hacer commit de los cambios:**
   ```bash
   git add -A
   git commit -m "feat/fix: descripción del cambio"
   ```

2. **Crear tag con nueva versión:**
   ```bash
   git tag -a v1.X.X -m "Descripción del release"
   ```

3. **Push del commit y tag:**
   ```bash
   git push origin main
   git push origin v1.X.X
   ```

4. **Generar build de release:**
   ```powershell
   .\build.ps1 -Release -Clean
   ```

5. **Crear ZIP para distribución:**
   ```powershell
   cd publish
   Compress-Archive -Path 'service/PrintAgent.Service.exe', 'service/appsettings.json', 'service/appsettings.Development.json' -DestinationPath 'PrintAgent-vX.X.X.zip' -Force
   ```

6. **Subir el ZIP a GitHub Releases** (manualmente o con `gh` CLI si está instalado)

### Versionado Semántico

- **MAJOR** (v2.0.0): Cambios incompatibles con versiones anteriores
- **MINOR** (v1.5.0): Nueva funcionalidad compatible hacia atrás
- **PATCH** (v1.4.6): Correcciones de bugs

## Comandos de Desarrollo

```powershell
# Compilar en modo Debug
dotnet build src/PrintAgent.Service

# Compilar en modo Release
.\build.ps1 -Release

# Compilar limpiando builds anteriores
.\build.ps1 -Release -Clean

# Ejecutar en desarrollo
dotnet run --project src/PrintAgent.Service
```

## API Endpoints

- `GET /health` - Estado del servicio
- `GET /printers` - Lista de impresoras configuradas
- `POST /print/bill` - Imprimir factura
- `POST /print/pre-bill` - Imprimir pre-cuenta
- `POST /print/kitchen` - Imprimir comanda de cocina
- `POST /print/test` - Página de prueba

## Modelos Principales

### BillData (PrintRequest.cs)
Datos de la factura para impresión:
- Información del cliente (nombre, teléfono, dirección)
- Datos de facturación (cédula, nombre comercial, correo)
- Items con modificadores
- Totales y pagos

### BusinessInfo (PrinterConfig.cs)
Configuración del negocio:
- Nombre, dirección, teléfono
- Logo, mensaje de agradecimiento
- Leyenda legal

## Notas

- El servicio escucha en `http://localhost:5123` por defecto
- Usa encoding ISO-8859-1 (Latin1) para compatibilidad con impresoras térmicas
- Los acentos se normalizan automáticamente (á → a, ñ → n, etc.)
