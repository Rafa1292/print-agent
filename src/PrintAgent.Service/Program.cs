using System.Net;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PrintAgent.Service.Models;
using PrintAgent.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar como Windows Service
builder.Host.UseWindowsService();

// Configurar JSON para aceptar enums como strings
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Cargar configuración
builder.Services.Configure<PrintAgentSettings>(
    builder.Configuration.GetSection("PrintAgent"));

// Registrar servicios
builder.Services.AddSingleton<ILogoCacheService, LogoCacheService>();
builder.Services.AddSingleton<IPrinterService, PrinterService>();
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

// Configurar CORS para permitir llamadas desde la web local
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                var uri = new Uri(origin);
                return uri.Host == "localhost"
                    || uri.Host == "127.0.0.1"
                    || uri.Host.StartsWith("192.168.")
                    || uri.Host.StartsWith("10.")
                    || uri.Host == "nicomanager.com"
                    || uri.Host.EndsWith(".nicomanager.com");
            })
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Usar CORS
app.UseCors("AllowLocalhost");

// Variable para tracking de inicio
var startTime = DateTime.Now;

// ============================================================================
// ENDPOINTS
// ============================================================================

// GET /network-info - IPs locales de la máquina
app.MapGet("/network-info", (IOptions<PrintAgentSettings> settings) =>
{
    var port = settings.Value.Port;
    var ips = Dns.GetHostEntry(Dns.GetHostName())
        .AddressList
        .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
        .Select(a => a.ToString())
        .ToList();

    return Results.Ok(new
    {
        port,
        ips,
        urls = ips.Select(ip => $"http://{ip}:{port}").ToList()
    });
});

// GET /health - Status del agente
app.MapGet("/health", (IPrinterService printerService, IOptions<PrintAgentSettings> settings) =>
{
    var status = new HealthStatus
    {
        IsRunning = true,
        Version = "1.0.0",
        StartTime = startTime,
        ConfiguredPrinters = printerService.GetConfiguredPrinters()
    };
    return Results.Ok(status);
});

// GET /printers - Lista impresoras configuradas
app.MapGet("/printers", (IPrinterService printerService) =>
{
    var printers = printerService.GetConfiguredPrinters();
    return Results.Ok(printers);
});

// GET /printers/system - Lista impresoras del sistema Windows
app.MapGet("/printers/system", (IPrinterService printerService) =>
{
    var printers = printerService.GetSystemPrinters();
    return Results.Ok(printers);
});

// POST /print/bill - Imprime factura
app.MapPost("/print/bill", async (PrintBillRequest request, IPrinterService printerService, ILogoCacheService logoCache) =>
{
    var result = await printerService.PrintBillAsync(request, logoCache);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// POST /print/pre-bill - Imprime pre-cuenta
app.MapPost("/print/pre-bill", async (PrintPreBillRequest request, IPrinterService printerService, ILogoCacheService logoCache) =>
{
    var result = await printerService.PrintPreBillAsync(request, logoCache);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// POST /print/kitchen - Imprime comanda de cocina
app.MapPost("/print/kitchen", (PrintKitchenRequest request, IPrinterService printerService) =>
{
    var result = printerService.PrintKitchenOrder(request);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// POST /print/test - Imprime página de prueba
app.MapPost("/print/test", (PrintTestRequest request, IPrinterService printerService) =>
{
    var success = printerService.TestPrinter(request.PrinterName);
    var result = success
        ? PrintResult.Ok("Página de prueba impresa correctamente")
        : PrintResult.Error("Error al imprimir página de prueba", "TEST_FAILED");
    return success ? Results.Ok(result) : Results.BadRequest(result);
});

// ============================================================================
// ENDPOINTS DE CONFIGURACIÓN
// ============================================================================

// POST /printers - Agregar nueva impresora
app.MapPost("/printers", (PrinterConfig printer, IConfigurationService configService) =>
{
    var result = configService.AddPrinter(printer);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// PUT /printers/{name} - Actualizar impresora existente
app.MapPut("/printers/{name}", (string name, PrinterConfig printer, IConfigurationService configService) =>
{
    var result = configService.UpdatePrinter(name, printer);
    if (result.ErrorCode == "NOT_FOUND")
    {
        return Results.NotFound(result);
    }
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// DELETE /printers/{name} - Eliminar impresora
app.MapDelete("/printers/{name}", (string name, IConfigurationService configService) =>
{
    var result = configService.DeletePrinter(name);
    if (result.ErrorCode == "NOT_FOUND")
    {
        return Results.NotFound(result);
    }
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// GET /business - Obtener información del negocio
app.MapGet("/business", (IConfigurationService configService) =>
{
    var business = configService.GetBusinessInfo();
    return Results.Ok(business);
});

// PUT /business - Actualizar información del negocio
app.MapPut("/business", (BusinessInfo business, IConfigurationService configService) =>
{
    var result = configService.UpdateBusinessInfo(business);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// GET /config - Obtener toda la configuración
app.MapGet("/config", (IConfigurationService configService) =>
{
    var settings = configService.GetSettings();
    return Results.Ok(settings);
});

app.Run();

// ============================================================================
// Request adicional para test
// ============================================================================
public class PrintTestRequest
{
    public string PrinterName { get; set; } = "factura";
}
