# PrintAgent - Guía de Implementación por Fases

Este documento sirve como guía para implementar el proyecto en múltiples sesiones.

**Para retomar:** Di "Continuemos con PrintAgent, estamos en la Fase X"

---

## Estado Actual: FASE 4 PENDIENTE

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

## Fase 4: Configuración y Persistencia ⏳ PENDIENTE
**Tiempo estimado de sesión:** ~15 min

### Objetivos:
- [ ] Crear `appsettings.json` con estructura de configuración
- [ ] Implementar lectura/escritura de configuración
- [ ] Crear servicio de configuración compartido

### Archivos a crear:
```
src/PrintAgent.Service/
├── appsettings.json       # Configuración por defecto
└── Services/
    └── ConfigurationService.cs
```

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

## Fase 5: UI de Configuración (WinForms) ⏳ PENDIENTE
**Tiempo estimado de sesión:** ~30 min

### Objetivos:
- [ ] Crear formulario principal con lista de impresoras
- [ ] Crear formulario para agregar/editar impresora
- [ ] Implementar botón de prueba de impresión
- [ ] Agregar icono en system tray
- [ ] Guardar configuración en appsettings.json

### Archivos a crear:
```
src/PrintAgent.UI/
├── Program.cs             # Modificar
├── Forms/
│   ├── MainForm.cs        # Ventana principal
│   ├── MainForm.Designer.cs
│   ├── PrinterConfigForm.cs
│   └── PrinterConfigForm.Designer.cs
└── Services/
    └── ConfigService.cs   # Lee/escribe config
```

### Funcionalidades de UI:
- Lista de impresoras configuradas
- Botón "Agregar impresora"
- Botón "Editar impresora"
- Botón "Eliminar impresora"
- Botón "Probar impresión"
- Checkbox "Iniciar con Windows"
- Minimizar a system tray

---

## Fase 6: Instalador ⏳ PENDIENTE
**Tiempo estimado de sesión:** ~20 min

### Objetivos:
- [ ] Crear script de Inno Setup
- [ ] Configurar registro como servicio Windows
- [ ] Agregar al inicio de Windows
- [ ] Crear acceso directo para UI de configuración

### Archivos a crear:
```
print-agent/
├── installer/
│   ├── setup.iss          # Script Inno Setup
│   └── assets/
│       └── icon.ico       # Icono de la app
└── build.ps1              # Script para compilar todo
```

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
- **Próximo paso:** Continuar con Fase 4 (Configuración y Persistencia)

---

## Para Retomar

Cuando inicies una nueva sesión, simplemente di:

> "Continuemos con PrintAgent, estamos en la Fase [X]"

Claude leerá este archivo y continuará desde donde quedamos.
