using Microsoft.Extensions.Options;
using PrintAgent.Service.Models;
using PrintAgent.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar como Windows Service
builder.Host.UseWindowsService();

// Cargar configuración
builder.Services.Configure<PrintAgentSettings>(
    builder.Configuration.GetSection("PrintAgent"));

// Registrar servicios
builder.Services.AddSingleton<IPrinterService, PrinterService>();

// Configurar CORS para permitir llamadas desde la web local
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                var uri = new Uri(origin);
                return uri.Host == "localhost" || uri.Host == "127.0.0.1";
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
app.MapPost("/print/bill", (PrintBillRequest request, IPrinterService printerService) =>
{
    var result = printerService.PrintBill(request);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// POST /print/pre-bill - Imprime pre-cuenta
app.MapPost("/print/pre-bill", (PrintPreBillRequest request, IPrinterService printerService) =>
{
    var result = printerService.PrintPreBill(request);
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

app.Run();

// ============================================================================
// Request adicional para test
// ============================================================================
public class PrintTestRequest
{
    public string PrinterName { get; set; } = "factura";
}
