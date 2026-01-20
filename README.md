# PrintAgent

Servicio de impresión local para impresoras térmicas. Permite que aplicaciones web impriman tickets y comandas directamente en impresoras POS conectadas a la computadora del usuario.

## Características

- **Impresión ESC/POS** - Comandos estándar para impresoras térmicas
- **API REST** - Endpoints HTTP para integración con aplicaciones web
- **Servicio Windows** - Corre en segundo plano automáticamente
- **Multi-impresora** - Soporte para múltiples impresoras (facturas, cocina, etc.)
- **Configuración remota** - Las impresoras se configuran desde la aplicación web

## Instalación

### Opción 1: Instalador (Recomendado)

1. Descargar el instalador desde [Releases](https://github.com/Rafa1292/print-agent/releases/latest)
2. Ejecutar `PrintAgent-Setup-x.x.x.exe` como administrador
3. Seguir el asistente de instalación

El servicio se iniciará automáticamente después de la instalación.

### Opción 2: Manual (Desarrollo)

```powershell
# Clonar repositorio
git clone https://github.com/Rafa1292/print-agent.git
cd print-agent

# Compilar
.\build.ps1 -Release

# Instalar servicio (como Admin)
.\scripts\install-service.ps1
```

## Configuración de impresoras

Las impresoras se configuran desde la aplicación web (nico) en **Configuración > Impresión**:

1. Instalar PrintAgent en la computadora
2. Abrir nico en el navegador
3. Ir a Configuración > Impresión
4. Hacer clic en "Agregar impresora"
5. Seleccionar la impresora del sistema y configurar nombre, tipo y ancho

## Uso

### Desde la web (JavaScript/TypeScript)

```typescript
// Verificar conexión
const response = await fetch('http://localhost:5123/health');
const health = await response.json();
console.log(health.isRunning); // true

// Imprimir factura
await fetch('http://localhost:5123/print/bill', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    printerName: 'factura',
    bill: {
      orderNumber: 1234,
      total: 50.00,
      items: [...]
    }
  })
});
```

### Endpoints disponibles

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/health` | Estado del servicio |
| GET | `/printers` | Impresoras configuradas |
| GET | `/printers/system` | Impresoras del sistema Windows |
| POST | `/print/bill` | Imprimir factura |
| POST | `/print/pre-bill` | Imprimir pre-cuenta |
| POST | `/print/kitchen` | Imprimir comanda de cocina |
| POST | `/print/test` | Página de prueba |
| GET | `/config` | Configuración completa |
| POST | `/printers` | Agregar impresora |
| PUT | `/printers/{name}` | Actualizar impresora |
| DELETE | `/printers/{name}` | Eliminar impresora |

## Configuración

La configuración se almacena en `appsettings.json`:

```json
{
  "PrintAgent": {
    "Port": 5123,
    "Printers": [
      {
        "Name": "factura",
        "SystemName": "EPSON TM-T20",
        "PaperWidth": 42,
        "Type": "Receipt",
        "IsDefault": true
      }
    ],
    "Business": {
      "Name": "Mi Negocio",
      "Address": "Calle Principal 123",
      "Phone": "555-1234",
      "TaxId": "12345678901"
    }
  }
}
```

### Ancho de papel

| Tamaño papel | Caracteres |
|--------------|------------|
| 58mm | 32-42 |
| 80mm | 42-48 |

## Desarrollo

### Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11

### Compilar

```powershell
# Debug
dotnet build

# Release
.\build.ps1 -Release
```

### Ejecutar en desarrollo

```powershell
cd src/PrintAgent.Service
dotnet run
```

### Probar endpoints

```powershell
.\scripts\test-endpoints.ps1
```

## Crear Release

```powershell
# Automático (via GitHub Actions)
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0

# Manual
.\scripts\publish-release.ps1 -Version "1.0.0"
```

## Estructura del proyecto

```
print-agent/
├── src/
│   └── PrintAgent.Service/     # Servicio Windows + API REST
│       ├── Models/             # DTOs y configuración
│       ├── Services/           # Lógica de negocio
│       └── Program.cs          # Entry point con endpoints
├── installer/                  # Script Inno Setup
├── scripts/                    # Scripts de utilidad
└── docs/                       # Documentación adicional
```

## Troubleshooting

Ver [TROUBLESHOOTING.md](TROUBLESHOOTING.md) para soluciones a problemas comunes.

## Licencia

MIT
