using System.Net.Http.Json;
using System.Text.Json;
using PrintAgent.UI.Models;

namespace PrintAgent.UI.Services;

/// <summary>
/// Cliente HTTP para comunicarse con la API de PrintAgent.Service
/// </summary>
public class PrintAgentClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PrintAgentClient(int port = 5123)
    {
        _baseUrl = $"http://localhost:{port}";
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    /// <summary>
    /// Verifica si el servicio está corriendo
    /// </summary>
    public async Task<HealthStatus?> GetHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<HealthStatus>(JsonOptions);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Obtiene la lista de impresoras configuradas
    /// </summary>
    public async Task<List<PrinterInfo>> GetPrintersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/printers");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<PrinterInfo>>(JsonOptions) ?? new();
            }
            return new();
        }
        catch
        {
            return new();
        }
    }

    /// <summary>
    /// Obtiene la lista de impresoras del sistema Windows
    /// </summary>
    public async Task<List<string>> GetSystemPrintersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/printers/system");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<string>>(JsonOptions) ?? new();
            }
            return new();
        }
        catch
        {
            return new();
        }
    }

    /// <summary>
    /// Agrega una nueva impresora
    /// </summary>
    public async Task<ConfigResult> AddPrinterAsync(PrinterConfig printer)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/printers", printer);
            var result = await response.Content.ReadFromJsonAsync<ConfigResult>(JsonOptions);
            return result ?? new ConfigResult { Success = false, Message = "Error de comunicación" };
        }
        catch (Exception ex)
        {
            return new ConfigResult { Success = false, Message = ex.Message, ErrorCode = "CONNECTION_ERROR" };
        }
    }

    /// <summary>
    /// Actualiza una impresora existente
    /// </summary>
    public async Task<ConfigResult> UpdatePrinterAsync(string name, PrinterConfig printer)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/printers/{Uri.EscapeDataString(name)}", printer);
            var result = await response.Content.ReadFromJsonAsync<ConfigResult>(JsonOptions);
            return result ?? new ConfigResult { Success = false, Message = "Error de comunicación" };
        }
        catch (Exception ex)
        {
            return new ConfigResult { Success = false, Message = ex.Message, ErrorCode = "CONNECTION_ERROR" };
        }
    }

    /// <summary>
    /// Elimina una impresora
    /// </summary>
    public async Task<ConfigResult> DeletePrinterAsync(string name)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/printers/{Uri.EscapeDataString(name)}");
            var result = await response.Content.ReadFromJsonAsync<ConfigResult>(JsonOptions);
            return result ?? new ConfigResult { Success = false, Message = "Error de comunicación" };
        }
        catch (Exception ex)
        {
            return new ConfigResult { Success = false, Message = ex.Message, ErrorCode = "CONNECTION_ERROR" };
        }
    }

    /// <summary>
    /// Obtiene la información del negocio
    /// </summary>
    public async Task<BusinessInfo?> GetBusinessInfoAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/business");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BusinessInfo>(JsonOptions);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Actualiza la información del negocio
    /// </summary>
    public async Task<ConfigResult> UpdateBusinessInfoAsync(BusinessInfo business)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync("/business", business);
            var result = await response.Content.ReadFromJsonAsync<ConfigResult>(JsonOptions);
            return result ?? new ConfigResult { Success = false, Message = "Error de comunicación" };
        }
        catch (Exception ex)
        {
            return new ConfigResult { Success = false, Message = ex.Message, ErrorCode = "CONNECTION_ERROR" };
        }
    }

    /// <summary>
    /// Imprime una página de prueba
    /// </summary>
    public async Task<ConfigResult> TestPrintAsync(string printerName)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/print/test", new { PrinterName = printerName });
            var result = await response.Content.ReadFromJsonAsync<ConfigResult>(JsonOptions);
            return result ?? new ConfigResult { Success = false, Message = "Error de comunicación" };
        }
        catch (Exception ex)
        {
            return new ConfigResult { Success = false, Message = ex.Message, ErrorCode = "CONNECTION_ERROR" };
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
