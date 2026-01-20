namespace PrintAgent.Service.Models;

public class PrintResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public static PrintResult Ok(string? message = null) => new()
    {
        Success = true,
        Message = message ?? "Impresión exitosa"
    };

    public static PrintResult Error(string message, string? errorCode = null) => new()
    {
        Success = false,
        Message = message,
        ErrorCode = errorCode
    };
}

public class PrinterInfo
{
    public string Name { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public int PaperWidth { get; set; }
    public bool IsOnline { get; set; }
    public bool IsDefault { get; set; }
    public PrinterType Type { get; set; }
}

public class HealthStatus
{
    public bool IsRunning { get; set; } = true;
    public string Version { get; set; } = "1.0.0";
    public DateTime StartTime { get; set; }
    public List<PrinterInfo> ConfiguredPrinters { get; set; } = new();
}
