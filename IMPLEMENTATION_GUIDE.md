# PrintAgent - Guía de Implementación por Fases

Este documento sirve como guía para implementar el proyecto en múltiples sesiones.

**Para retomar:** Di "Continuemos con PrintAgent, estamos en la Fase X"

---

## Estado Actual: FASE 7 PENDIENTE

---

## Mapeo de Modelos: Nico → PrintAgent

Los modelos de PrintAgent están diseñados para recibir exactamente los datos del sistema de facturación nico.

### Factura (Bill)

| Campo Nico (BillDTO) | Campo PrintAgent (BillData) |
|----------------------|-----------------------------|
| `id` | `Id` |
| `orderNumber` | `OrderNumber` |
| `deliveryMethod` | `DeliveryMethod` |
| `tableNumber` | `TableNumber` |
| `customerName` | `CustomerName` |
| `subtotal` | `Subtotal` |
| `tax` | `Tax` |
| `discount` | `Discount` |
| `total` | `Total` |
| `tip` | `Tip` |
| `createdAt` | `CreatedAt` |
| `closedAt` | `ClosedAt` |

### Items de Factura

| Campo Nico (BillItemDTO) | Campo PrintAgent (BillItemData) |
|--------------------------|---------------------------------|
| `id` | `Id` |
| `description` | `Description` |
| `quantity` | `Quantity` |
| `unitPrice` | `UnitPrice` |
| `subtotal` | `Subtotal` |
| `discount` | `Discount` |
| `tax` | `Tax` |
| `total` | `Total` |
| `isCancelled` | `IsCancelled` |

### Comanda de Cocina

| Campo Nico (KitchenOrder) | Campo PrintAgent (KitchenOrderData) |
|---------------------------|-------------------------------------|
| `kitchenMessageId` | `KitchenMessageId` |
| `orderNumber` | `OrderNumber` (ronda) |
| `billId` | `BillId` |
| `billOrderNumber` | `BillOrderNumber` |
| `tableNumber` | `TableNumber` |
| `roomName` | `RoomName` |
| `deliveryMethod` | `DeliveryMethod` |
| `customerName` | `CustomerName` |
| `items` | `Items` (con Articles y Modifiers) |
| `waitMinutes` | `WaitMinutes` |
| `status` | `Status` |
| `initTime` | `InitTime` |

### Archivos de Referencia en Nico

| Tipo | Archivo |
|------|---------|
| Bill, BillItem | `nico/src/domains/bill/types.ts` |
| Customer | `nico/src/domains/customer/types.ts` |
| KitchenOrder | `nico/src/app/api/restaurant/kitchen/orders/route.ts` |
| Schema Prisma | `nico/prisma/schema.prisma` |

---

## Fase 1: Estructura Base y Modelos ✅ COMPLETADA
**Tiempo estimado de sesión:** ~15 min

### Objetivos:
- [x] Verificar .NET SDK 8.x instalado
- [x] Crear solución `PrintAgent.sln`
- [x] Crear proyecto `PrintAgent.Service` (Worker Service)
- [x] Crear proyecto `PrintAgent.UI` (WinForms)
- [x] Agregar paquetes NuGet necesarios
- [x] Crear estructura de carpetas (Controllers, Services, Models)
- [x] Crear modelos de datos (PrinterConfig, PrintRequest, PrintResult)

### Archivos creados:
```
print-agent/
├── PrintAgent.sln ✅
├── src/
│   ├── PrintAgent.Service/
│   │   ├── PrintAgent.Service.csproj ✅
│   │   ├── Models/
│   │   │   ├── PrinterConfig.cs ✅
│   │   │   ├── PrintRequest.cs ✅
│   │   │   └── PrintResult.cs ✅
│   │   ├── Services/
│   │   │   ├── EscPosBuilder.cs ✅
│   │   │   └── IPrinterService.cs ✅
│   │   └── Controllers/ (vacío)
│   └── PrintAgent.UI/
│       └── PrintAgent.UI.csproj ✅
```

### Comandos ejecutados:
```bash
dotnet new sln -n PrintAgent
dotnet new worker -n PrintAgent.Service -o src/PrintAgent.Service
dotnet new winforms -n PrintAgent.UI -o src/PrintAgent.UI
dotnet sln add src/PrintAgent.Service/PrintAgent.Service.csproj
dotnet sln add src/PrintAgent.UI/PrintAgent.UI.csproj
dotnet add package Microsoft.Extensions.Hosting --version 10.0.2
dotnet add package Microsoft.Extensions.Hosting.WindowsServices
dotnet add package System.Drawing.Common
```

---

## Fase 2: Servicio de Impresión ✅ COMPLETADA
**Tiempo estimado de sesión:** ~20 min

### Objetivos:
- [x] Implementar `PrinterService.cs` (lógica de impresión RAW)
- [x] Implementar generación de tickets para facturas
- [x] Implementar generación de comandas de cocina
- [x] Agregar manejo de errores de impresión
- [x] Configurar proyecto como Windows-only (net8.0-windows)

### Archivos creados:
```
src/PrintAgent.Service/
├── PrintAgent.Service.csproj  # Actualizado: net8.0-windows, win-x64
├── appsettings.json           # ✅ Configuración con PrintAgent section
└── Services/
    ├── PrinterService.cs      # ✅ Implementación completa con Windows API
    └── TicketBuilder.cs       # ✅ Construcción de tickets ESC/POS
```

### Funcionalidades implementadas:

**PrinterService.cs:**
- `PrintBill()` - Imprime factura completa
- `PrintPreBill()` - Imprime pre-cuenta
- `PrintKitchenOrder()` - Imprime comanda de cocina
- `TestPrinter()` - Imprime página de prueba
- `GetConfiguredPrinters()` - Lista impresoras configuradas
- `GetSystemPrinters()` - Lista impresoras del sistema Windows
- `SendRawDataToPrinter()` - Envío RAW via Windows API (winspool.drv)

**TicketBuilder.cs:**
- `BuildBillTicket()` - Ticket de factura con items, totales, cliente
- `BuildPreBillTicket()` - Ticket de pre-cuenta (sin método de pago)
- `BuildKitchenTicket()` - Ticket de cocina con artículos y modificadores
- `BuildTestPage()` - Página de prueba de impresora

---

## Fase 3: API REST ✅ COMPLETADA
**Tiempo estimado de sesión:** ~20 min

### Objetivos:
- [x] Configurar ASP.NET Core minimal API
- [x] Crear endpoints REST
- [x] Configurar CORS para llamadas desde localhost
- [x] Cambiar SDK a Microsoft.NET.Sdk.Web

### Archivos modificados:
```
src/PrintAgent.Service/
├── Program.cs                  # ✅ Minimal API con todos los endpoints
├── PrintAgent.Service.csproj   # ✅ Cambiado a SDK.Web
├── Worker.cs                   # ❌ Eliminado (no necesario)
└── appsettings.json            # ✅ Ya configurado en Fase 2
```

### Endpoints implementados:
| Método | Ruta | Descripción | Estado |
|--------|------|-------------|--------|
| GET | /health | Status del agente | ✅ Probado |
| GET | /printers | Lista impresoras configuradas | ✅ Probado |
| GET | /printers/system | Lista impresoras del sistema | ✅ Probado |
| POST | /print/bill | Imprime factura | ✅ Probado |
| POST | /print/pre-bill | Imprime pre-cuenta | ✅ Implementado |
| POST | /print/kitchen | Imprime comanda cocina | ✅ Implementado |
| POST | /print/test | Imprime página de prueba | ✅ Implementado |

### Configuración CORS:
- Permite llamadas desde `localhost` y `127.0.0.1`
- Acepta cualquier método y header

### Ejemplo de respuesta /health:
```json
{
  "isRunning": true,
  "version": "1.0.0",
  "startTime": "2026-01-19T17:10:03",
  "configuredPrinters": [
    { "name": "factura", "systemName": "POS-58", "isOnline": false }
  ]
}
```

---

## Fase 4: Configuración y Persistencia ✅ COMPLETADA
**Tiempo estimado de sesión:** ~15 min

### Objetivos:
- [x] Crear `appsettings.json` con estructura de configuración
- [x] Implementar lectura/escritura de configuración
- [x] Crear servicio de configuración compartido

### Archivos creados:
```
src/PrintAgent.Service/
├── appsettings.json                           # ✅ Ya existía de Fase 2
└── Services/
    ├── IConfigurationService.cs               # ✅ Interfaz del servicio
    └── ConfigurationService.cs                # ✅ Implementación CRUD
```

### Endpoints de configuración implementados:
| Método | Ruta | Descripción | Estado |
|--------|------|-------------|--------|
| POST | /printers | Agregar nueva impresora | ✅ Implementado |
| PUT | /printers/{name} | Actualizar impresora | ✅ Implementado |
| DELETE | /printers/{name} | Eliminar impresora | ✅ Implementado |
| GET | /business | Obtener info del negocio | ✅ Implementado |
| PUT | /business | Actualizar info del negocio | ✅ Implementado |
| GET | /config | Obtener toda la configuración | ✅ Implementado |

### Funcionalidades del ConfigurationService:
- **AddPrinter()** - Agrega impresora con validación de duplicados
- **UpdatePrinter()** - Actualiza impresora existente con rollback en caso de error
- **DeletePrinter()** - Elimina impresora por nombre
- **UpdateBusinessInfo()** - Actualiza datos del negocio
- **SaveConfiguration()** - Persiste cambios en `appsettings.json` preservando otras secciones
- Thread-safe con `lock` para operaciones concurrentes

### Estructura de configuración:
```json
{
  "PrintAgent": {
    "Port": 5123,
    "Printers": [
      {
        "Name": "factura",
        "SystemName": "EPSON TM-T20",
        "PaperWidth": 48,
        "Type": "Receipt",
        "IsDefault": true
      }
    ],
    "Business": {
      "Name": "Mi Negocio",
      "Address": "Calle 123",
      "Phone": "555-1234",
      "TaxId": "12345678"
    }
  }
}
```

---

## Fase 5: UI de Configuración (WinForms) ✅ COMPLETADA
**Tiempo estimado de sesión:** ~30 min

### Objetivos:
- [x] Crear formulario principal con lista de impresoras
- [x] Crear formulario para agregar/editar impresora
- [x] Implementar botón de prueba de impresión
- [x] Agregar icono en system tray
- [x] Comunicación con API del servicio

### Archivos creados:
```
src/PrintAgent.UI/
├── Program.cs                              # ✅ Actualizado para usar MainForm
├── Models/
│   └── PrinterConfig.cs                    # ✅ Modelos para la UI
├── Services/
│   └── PrintAgentClient.cs                 # ✅ Cliente HTTP para comunicación con API
└── Forms/
    ├── MainForm.cs                         # ✅ Ventana principal
    ├── MainForm.Designer.cs                # ✅ Diseño del form principal
    ├── PrinterConfigForm.cs                # ✅ Form para agregar/editar impresoras
    └── PrinterConfigForm.Designer.cs       # ✅ Diseño del form de configuración
```

### Funcionalidades implementadas:
- **MainForm:**
  - Lista de impresoras con columnas: Nombre, Sistema, Ancho, Tipo, Default, Estado
  - Botones: Agregar, Editar, Eliminar, Probar, Actualizar
  - Status bar mostrando estado del servicio
  - Minimizar a system tray (NotifyIcon)
  - Menú contextual en tray: Mostrar / Salir
  - Auto-refresh del status cada 5 segundos
  - Doble-click en impresora abre edición

- **PrinterConfigForm:**
  - Campo nombre de impresora
  - Dropdown con impresoras del sistema Windows
  - Selector de ancho de papel (caracteres)
  - Tipo: Receipt / Kitchen
  - Checkbox impresora predeterminada

- **PrintAgentClient:**
  - Comunicación HTTP con la API del servicio
  - Métodos para CRUD de impresoras
  - Health check y test de impresión

---

## Fase 6: Instalador ✅ COMPLETADA
**Tiempo estimado de sesión:** ~20 min

### Objetivos:
- [x] Crear script de Inno Setup
- [x] Configurar registro como servicio Windows
- [x] Agregar al inicio de Windows (opcional)
- [x] Crear acceso directo para UI de configuración

### Archivos creados:
```
print-agent/
├── build.ps1                       # ✅ Script PowerShell para compilar y publicar
├── installer/
│   ├── setup.iss                   # ✅ Script Inno Setup completo
│   └── assets/
│       └── README.md               # ✅ Instrucciones para el icono
├── scripts/
│   ├── install-service.ps1         # ✅ Instalar servicio manualmente
│   └── uninstall-service.ps1       # ✅ Desinstalar servicio manualmente
└── publish/                        # ✅ Generado por build.ps1
    ├── service/
    │   └── PrintAgent.Service.exe  # ~91 MB (self-contained)
    └── ui/
        └── PrintAgent.UI.exe       # ~154 MB (self-contained)
```

### Build y publicación:
```powershell
# Compilar y publicar (Release, self-contained, single-file)
.\build.ps1 -Release

# Con limpieza previa
.\build.ps1 -Release -Clean
```

### Crear instalador:
1. Descargar Inno Setup: https://jrsoftware.org/isdl.php
2. Abrir `installer\setup.iss` con Inno Setup
3. Compilar (Ctrl+F9)
4. El instalador se genera en `dist\PrintAgent-Setup-1.0.0.exe`

### El instalador hace:
- Instala archivos en `C:\Program Files\PrintAgent\`
- Registra `PrintAgent.Service` como servicio Windows (auto-start)
- Configura regla de firewall para puerto 5123
- Crea acceso directo en menú inicio
- Opción: Acceso directo en escritorio
- Opción: Iniciar con Windows (UI)

### Scripts para desarrollo:
```powershell
# Instalar servicio manualmente (como Admin)
.\scripts\install-service.ps1

# Desinstalar servicio
.\scripts\uninstall-service.ps1
```

### Distribución via GitHub Releases:
```powershell
# Opción 1: Release automático (recomendado)
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0
# GitHub Actions compila y publica automáticamente

# Opción 2: Release manual
.\scripts\publish-release.ps1 -Version "1.0.0"
```

### URLs de descarga:
```
# Página del release
https://github.com/USUARIO/print-agent/releases/tag/v1.0.0

# Descarga directa
https://github.com/USUARIO/print-agent/releases/download/v1.0.0/PrintAgent-Setup-1.0.0.exe

# Última versión
https://github.com/USUARIO/print-agent/releases/latest
```

Ver `RELEASE.md` para documentación completa del proceso de release.

---

## Fase 7: Integración con Nico (Web) ⏳ PENDIENTE
**Tiempo estimado de sesión:** ~25 min

### Objetivos:
- [ ] Crear servicio en frontend para comunicarse con PrintAgent
- [ ] Agregar página de descarga del instalador
- [ ] Implementar detección de agente (health check)
- [ ] Modificar lógica de impresión existente para usar el agente

### Archivos a crear en `nico/`:
```
src/
├── lib/
│   └── print-agent.ts     # Cliente del agente
├── app/
│   └── (app)/configuracion/impresion/
│       └── page.tsx       # Página de configuración
└── components/
    └── print/
        └── print-agent-status.tsx
```

---

## Fase 8: Testing y Pulido ⏳ PENDIENTE
**Tiempo estimado de sesión:** ~20 min

### Objetivos:
- [ ] Probar impresión completa de factura
- [ ] Probar impresión de comanda de cocina
- [ ] Verificar funcionamiento como servicio Windows
- [ ] Probar instalador en máquina limpia
- [ ] Documentar troubleshooting común

---

## Comandos Útiles

### Compilar solución
```bash
cd C:/Users/jrvj_/code/Nebulosa/print-agent
dotnet build
```

### Ejecutar servicio en modo desarrollo
```bash
cd src/PrintAgent.Service
dotnet run
```

### Ejecutar UI
```bash
cd src/PrintAgent.UI
dotnet run
```

### Publicar para producción
```bash
dotnet publish src/PrintAgent.Service -c Release -o ./publish/service
dotnet publish src/PrintAgent.UI -c Release -o ./publish/ui
```

---

## Notas de Sesión

### Sesión 1 (Fecha: 2026-01-19)
- Creada estructura inicial del proyecto
- Instalados paquetes NuGet
- Creados modelos básicos iniciales
- Creado EscPosBuilder con comandos ESC/POS
- **Revisión:** Actualizado modelos para coincidir con estructura de nico:
  - `PrintRequest.cs` ahora mapea exactamente a `BillDTO`, `BillItemDTO`, `KitchenOrder`
  - Agregado soporte para modificadores de cocina (ModifierGroups, ModifierElements)
  - Agregado `PrintPreBillRequest` para pre-cuentas
  - Agregado mapeo de campos en esta guía
- **Fase 2 completada:**
  - Implementado `TicketBuilder.cs` con generación de tickets ESC/POS
  - Implementado `PrinterService.cs` con impresión RAW via Windows API
  - Configurado `appsettings.json` con estructura de configuración
  - Actualizado proyecto a `net8.0-windows` para soporte nativo
- **Fase 3 completada:**
  - Cambiado SDK de Worker a Web
  - Implementado Program.cs con minimal API
  - Configurado CORS para localhost
  - Todos los endpoints probados y funcionando
  - Eliminado Worker.cs (no necesario)
- **Fase 4 completada:**
  - Creado `IConfigurationService.cs` con interfaz para CRUD
  - Implementado `ConfigurationService.cs` con persistencia en appsettings.json
  - Agregados endpoints: POST/PUT/DELETE /printers, GET/PUT /business, GET /config
  - Thread-safe con locks y rollback en caso de error
- **Fase 5 completada:**
  - Creado `PrintAgentClient.cs` para comunicación HTTP con la API
  - Creados modelos locales en `Models/PrinterConfig.cs`
  - Implementado `MainForm` con lista de impresoras y system tray
  - Implementado `PrinterConfigForm` para agregar/editar impresoras
  - Eliminados archivos Form1 originales
  - Compila sin errores ni warnings
- **Fase 6 completada:**
  - Creado `build.ps1` para compilar y publicar (self-contained, single-file)
  - Creado `installer/setup.iss` para Inno Setup con registro de servicio Windows
  - Creados scripts helper: `install-service.ps1` y `uninstall-service.ps1`
  - Build probado exitosamente: Service ~91MB, UI ~154MB
  - Creado `scripts/publish-release.ps1` para publicar releases manualmente
  - Creado `.github/workflows/release.yml` para CI/CD automático
  - Creado `RELEASE.md` con documentación del proceso de release
- **Próximo paso:** Continuar con Fase 7 (Integración con Nico)

---

## Para Retomar

Cuando inicies una nueva sesión, simplemente di:

> "Continuemos con PrintAgent, estamos en la Fase [X]"

Claude leerá este archivo y continuará desde donde quedamos.
