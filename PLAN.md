# PrintAgent - Plan de Implementación

## Contexto del Proyecto

PrintAgent es un servicio Windows en C# que permite a la aplicación web "nico" (facturación en Next.js) imprimir en impresoras térmicas locales.

### Flujo de comunicación
```
┌─────────────────┐     HTTP      ┌─────────────────┐     ESC/POS    ┌─────────────┐
│   Web (nico)    │ ───────────►  │   PrintAgent    │ ─────────────► │  Impresora  │
│   en la nube    │  localhost    │   (C# local)    │    RAW         │   térmica   │
└─────────────────┘               └─────────────────┘                └─────────────┘
```

## Decisiones Tomadas

| Decisión | Elección | Razón |
|----------|----------|-------|
| Protocolo impresión | ESC/POS directo | Estándar de la industria, más compatible |
| Configuración | UI local | Fácil para el usuario, sin tocar archivos |
| Distribución | MSI Installer | Profesional, fácil de distribuir |
| Auto-inicio | Servicio Windows | Corre en background automáticamente |
| Ubicación código | Carpeta hermana | Proyectos independientes, fácil de manejar |

## Problemas de la App Actual (a resolver)

1. **Sin feedback real** - Solo devuelve `success: true` sin verificar impresión
2. **Impresoras hardcodeadas** - Nombres "factura" y "cocina" fijos en código
3. **Ancho hardcodeado** - 284px en varios lugares
4. **Race conditions** - Estado en variables de instancia
5. **Sin ESC/POS** - Usa System.Drawing (más pesado, menos compatible)
6. **Sin configuración externa** - Todo en código

## Arquitectura Nueva

```
PrintAgent (C#)
├── API Layer
│   ├── POST /print/bill      → Imprime factura
│   ├── POST /print/command   → Imprime comando cocina
│   ├── GET  /printers        → Lista impresoras disponibles
│   └── GET  /health          → Status del agente
├── Print Engine
│   ├── ESC/POS Commands      → Comandos estándar térmicas
│   ├── Template Builder      → Construye el ticket
│   └── Printer Manager       → Gestiona impresoras
└── Config (appsettings.json)
    ├── Printers[]            → Array de impresoras configuradas
    ├── PaperWidth            → Ancho configurable
    └── BusinessInfo          → Datos del negocio
```

## Estructura de Carpetas a Crear

```
print-agent/
├── PrintAgent.sln
├── src/
│   ├── PrintAgent.Service/           # Servicio Windows + API REST
│   │   ├── Program.cs                # Entry point, configura host
│   │   ├── Worker.cs                 # Background service
│   │   ├── PrintAgent.Service.csproj
│   │   ├── appsettings.json          # Configuración
│   │   ├── Controllers/
│   │   │   └── PrintController.cs    # Endpoints de impresión
│   │   ├── Services/
│   │   │   ├── IPrinterService.cs
│   │   │   ├── PrinterService.cs     # Lógica de impresión
│   │   │   └── EscPosBuilder.cs      # Comandos ESC/POS
│   │   └── Models/
│   │       ├── PrintRequest.cs       # DTOs de entrada
│   │       ├── PrintResult.cs        # DTOs de salida
│   │       └── PrinterConfig.cs      # Config de impresoras
│   │
│   └── PrintAgent.UI/                # App WinForms para configuración
│       ├── Program.cs
│       ├── PrintAgent.UI.csproj
│       ├── Forms/
│       │   ├── MainForm.cs           # Ventana principal
│       │   └── PrinterConfigForm.cs  # Configurar impresoras
│       └── Services/
│           └── ConfigService.cs      # Lee/escribe config
│
├── installer/                         # Scripts para crear MSI
│   └── setup.iss                     # Inno Setup script
│
└── README.md
```

## Pasos Siguientes

### Paso 1: Verificar .NET SDK
```bash
dotnet --version
# Debe mostrar 8.x.x
```

### Paso 2: Crear la solución y proyectos
```bash
cd C:/Users/jrvj_/code/Nebulosa/print-agent

# Crear solución
dotnet new sln -n PrintAgent

# Crear proyecto del servicio
dotnet new worker -n PrintAgent.Service -o src/PrintAgent.Service
dotnet sln add src/PrintAgent.Service/PrintAgent.Service.csproj

# Crear proyecto de UI
dotnet new winforms -n PrintAgent.UI -o src/PrintAgent.UI
dotnet sln add src/PrintAgent.UI/PrintAgent.UI.csproj

# Agregar paquetes necesarios
cd src/PrintAgent.Service
dotnet add package Microsoft.Extensions.Hosting.WindowsServices
dotnet add package Microsoft.AspNetCore.OpenApi
```

### Paso 3: Implementar el servicio
- Configurar ASP.NET Core minimal API dentro del Worker Service
- Implementar EscPosBuilder con comandos estándar
- Implementar PrinterService para enviar a impresora RAW
- Agregar endpoints REST

### Paso 4: Implementar UI de configuración
- Form para listar/agregar/editar impresoras
- Guardar en appsettings.json compartido
- Botón para probar impresión

### Paso 5: Crear instalador MSI
- Usar Inno Setup o WiX Toolset
- Registrar como servicio Windows
- Agregar al inicio de Windows

### Paso 6: Integrar con nico (web)
- Crear página de descarga del instalador
- Agregar lógica en frontend para llamar a localhost:PUERTO
- Manejar caso donde el agente no está corriendo

## Paquetes NuGet a Usar

| Paquete | Propósito |
|---------|-----------|
| Microsoft.Extensions.Hosting.WindowsServices | Correr como servicio Windows |
| System.IO.Ports | Comunicación serial (si se necesita) |
| System.Drawing.Common | Solo si se necesita generar imágenes |

## Comandos ESC/POS Básicos

```csharp
public static class EscPos
{
    public static byte[] Initialize => new byte[] { 0x1B, 0x40 };
    public static byte[] Cut => new byte[] { 0x1D, 0x56, 0x00 };
    public static byte[] BoldOn => new byte[] { 0x1B, 0x45, 0x01 };
    public static byte[] BoldOff => new byte[] { 0x1B, 0x45, 0x00 };
    public static byte[] AlignCenter => new byte[] { 0x1B, 0x61, 0x01 };
    public static byte[] AlignLeft => new byte[] { 0x1B, 0x61, 0x00 };
    public static byte[] AlignRight => new byte[] { 0x1B, 0x61, 0x02 };
    public static byte[] DoubleHeight => new byte[] { 0x1B, 0x21, 0x10 };
    public static byte[] NormalSize => new byte[] { 0x1B, 0x21, 0x00 };
    public static byte[] LineFeed => new byte[] { 0x0A };
}
```

## Ejemplo de Request para Factura

```json
POST /print/bill
{
  "printerName": "factura",
  "bill": {
    "number": "F001-00001234",
    "date": "2024-01-15T14:30:00",
    "customer": {
      "name": "Juan Pérez",
      "document": "12345678"
    },
    "items": [
      { "name": "Producto 1", "qty": 2, "price": 10.50, "total": 21.00 },
      { "name": "Producto 2", "qty": 1, "price": 5.00, "total": 5.00 }
    ],
    "subtotal": 26.00,
    "tax": 4.68,
    "total": 30.68,
    "payMethod": "Efectivo"
  }
}
```

## Puerto por Defecto

El servicio escuchará en `http://localhost:5123` por defecto.
La web llamará a este puerto para imprimir.

---

## Para Continuar

Cuando reinicies y abras Claude Code, simplemente di:

> "Continuemos con el proyecto print-agent, ya instalé .NET SDK"

Claude leerá este archivo y continuará desde el Paso 2.
