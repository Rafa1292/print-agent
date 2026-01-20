using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrintAgent.Service.Models;

namespace PrintAgent.Service.Services;

/// <summary>
/// Servicio para gestionar la configuración del agente de impresión.
/// Lee y escribe en appsettings.json
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _configFilePath;
    private PrintAgentSettings _settings;
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null, // Mantener PascalCase
        Converters = { new JsonStringEnumConverter() }
    };

    public ConfigurationService(
        ILogger<ConfigurationService> logger,
        IOptions<PrintAgentSettings> settings,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _settings = settings.Value;

        // Determinar la ruta del archivo de configuración
        _configFilePath = Path.Combine(environment.ContentRootPath, "appsettings.json");

        _logger.LogInformation("ConfigurationService inicializado. Archivo: {Path}", _configFilePath);
    }

    public PrintAgentSettings GetSettings()
    {
        lock (_lock)
        {
            return _settings;
        }
    }

    public List<PrinterConfig> GetPrinters()
    {
        lock (_lock)
        {
            return _settings.Printers.ToList();
        }
    }

    public PrinterConfig? GetPrinter(string name)
    {
        lock (_lock)
        {
            return _settings.Printers.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    public ConfigResult AddPrinter(PrinterConfig printer)
    {
        lock (_lock)
        {
            // Validar que no exista
            if (_settings.Printers.Any(p => p.Name.Equals(printer.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return ConfigResult.Error($"Ya existe una impresora con el nombre '{printer.Name}'", "DUPLICATE_NAME");
            }

            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(printer.Name))
            {
                return ConfigResult.Error("El nombre de la impresora es requerido", "INVALID_NAME");
            }

            if (string.IsNullOrWhiteSpace(printer.SystemName))
            {
                return ConfigResult.Error("El nombre del sistema es requerido", "INVALID_SYSTEM_NAME");
            }

            // Si es default, quitar default de las demás
            if (printer.IsDefault)
            {
                foreach (var p in _settings.Printers)
                {
                    p.IsDefault = false;
                }
            }

            _settings.Printers.Add(printer);

            if (!SaveConfiguration())
            {
                _settings.Printers.Remove(printer);
                return ConfigResult.Error("Error al guardar la configuración", "SAVE_ERROR");
            }

            _logger.LogInformation("Impresora '{Name}' agregada correctamente", printer.Name);
            return ConfigResult.Ok($"Impresora '{printer.Name}' agregada correctamente");
        }
    }

    public ConfigResult UpdatePrinter(string name, PrinterConfig printer)
    {
        lock (_lock)
        {
            var existing = _settings.Printers.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                return ConfigResult.Error($"Impresora '{name}' no encontrada", "NOT_FOUND");
            }

            // Si cambió el nombre, verificar que no exista otro con ese nombre
            if (!existing.Name.Equals(printer.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (_settings.Printers.Any(p => p.Name.Equals(printer.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return ConfigResult.Error($"Ya existe una impresora con el nombre '{printer.Name}'", "DUPLICATE_NAME");
                }
            }

            // Guardar valores originales para rollback
            var originalName = existing.Name;
            var originalSystemName = existing.SystemName;
            var originalPaperWidth = existing.PaperWidth;
            var originalIsDefault = existing.IsDefault;
            var originalType = existing.Type;

            // Si es default, quitar default de las demás
            if (printer.IsDefault && !existing.IsDefault)
            {
                foreach (var p in _settings.Printers.Where(p => p != existing))
                {
                    p.IsDefault = false;
                }
            }

            // Actualizar
            existing.Name = printer.Name;
            existing.SystemName = printer.SystemName;
            existing.PaperWidth = printer.PaperWidth;
            existing.IsDefault = printer.IsDefault;
            existing.Type = printer.Type;

            if (!SaveConfiguration())
            {
                // Rollback
                existing.Name = originalName;
                existing.SystemName = originalSystemName;
                existing.PaperWidth = originalPaperWidth;
                existing.IsDefault = originalIsDefault;
                existing.Type = originalType;
                return ConfigResult.Error("Error al guardar la configuración", "SAVE_ERROR");
            }

            _logger.LogInformation("Impresora '{Name}' actualizada correctamente", printer.Name);
            return ConfigResult.Ok($"Impresora '{printer.Name}' actualizada correctamente");
        }
    }

    public ConfigResult DeletePrinter(string name)
    {
        lock (_lock)
        {
            var printer = _settings.Printers.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (printer == null)
            {
                return ConfigResult.Error($"Impresora '{name}' no encontrada", "NOT_FOUND");
            }

            _settings.Printers.Remove(printer);

            if (!SaveConfiguration())
            {
                _settings.Printers.Add(printer);
                return ConfigResult.Error("Error al guardar la configuración", "SAVE_ERROR");
            }

            _logger.LogInformation("Impresora '{Name}' eliminada correctamente", name);
            return ConfigResult.Ok($"Impresora '{name}' eliminada correctamente");
        }
    }

    public BusinessInfo GetBusinessInfo()
    {
        lock (_lock)
        {
            return _settings.Business;
        }
    }

    public ConfigResult UpdateBusinessInfo(BusinessInfo business)
    {
        lock (_lock)
        {
            // Guardar original para rollback
            var original = _settings.Business;

            _settings.Business = business;

            if (!SaveConfiguration())
            {
                _settings.Business = original;
                return ConfigResult.Error("Error al guardar la configuración", "SAVE_ERROR");
            }

            _logger.LogInformation("Información del negocio actualizada");
            return ConfigResult.Ok("Información del negocio actualizada correctamente");
        }
    }

    public bool SaveConfiguration()
    {
        try
        {
            // Leer el archivo actual para preservar otras secciones
            string jsonContent;
            JsonNode? rootNode;

            if (File.Exists(_configFilePath))
            {
                jsonContent = File.ReadAllText(_configFilePath);
                rootNode = JsonNode.Parse(jsonContent) ?? new JsonObject();
            }
            else
            {
                rootNode = new JsonObject();
            }

            // Actualizar solo la sección PrintAgent
            var printAgentNode = new JsonObject
            {
                ["Port"] = _settings.Port,
                ["Printers"] = JsonSerializer.SerializeToNode(_settings.Printers, JsonOptions),
                ["Business"] = JsonSerializer.SerializeToNode(_settings.Business, JsonOptions)
            };

            rootNode["PrintAgent"] = printAgentNode;

            // Escribir el archivo
            var outputJson = rootNode.ToJsonString(JsonOptions);
            File.WriteAllText(_configFilePath, outputJson);

            _logger.LogDebug("Configuración guardada en {Path}", _configFilePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar configuración en {Path}", _configFilePath);
            return false;
        }
    }
}
