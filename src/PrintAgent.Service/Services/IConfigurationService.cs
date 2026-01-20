using PrintAgent.Service.Models;

namespace PrintAgent.Service.Services;

/// <summary>
/// Servicio para gestionar la configuración del agente de impresión
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Obtiene la configuración actual
    /// </summary>
    PrintAgentSettings GetSettings();

    /// <summary>
    /// Obtiene todas las impresoras configuradas
    /// </summary>
    List<PrinterConfig> GetPrinters();

    /// <summary>
    /// Obtiene una impresora por nombre
    /// </summary>
    PrinterConfig? GetPrinter(string name);

    /// <summary>
    /// Agrega una nueva impresora
    /// </summary>
    ConfigResult AddPrinter(PrinterConfig printer);

    /// <summary>
    /// Actualiza una impresora existente
    /// </summary>
    ConfigResult UpdatePrinter(string name, PrinterConfig printer);

    /// <summary>
    /// Elimina una impresora
    /// </summary>
    ConfigResult DeletePrinter(string name);

    /// <summary>
    /// Obtiene la información del negocio
    /// </summary>
    BusinessInfo GetBusinessInfo();

    /// <summary>
    /// Actualiza la información del negocio
    /// </summary>
    ConfigResult UpdateBusinessInfo(BusinessInfo business);

    /// <summary>
    /// Guarda los cambios en el archivo de configuración
    /// </summary>
    bool SaveConfiguration();
}

/// <summary>
/// Resultado de operaciones de configuración
/// </summary>
public class ConfigResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }

    public static ConfigResult Ok(string message = "Operación exitosa") =>
        new() { Success = true, Message = message };

    public static ConfigResult Error(string message, string code) =>
        new() { Success = false, Message = message, ErrorCode = code };
}
