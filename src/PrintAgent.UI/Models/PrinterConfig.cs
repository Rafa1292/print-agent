namespace PrintAgent.UI.Models;

public class PrinterConfig
{
    public string Name { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public int PaperWidth { get; set; } = 42;
    public bool IsDefault { get; set; }
    public string Type { get; set; } = "Receipt";
}

public class PrinterInfo : PrinterConfig
{
    public bool IsOnline { get; set; }
}

public class BusinessInfo
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
}

public class PrintAgentSettings
{
    public int Port { get; set; } = 5123;
    public List<PrinterConfig> Printers { get; set; } = new();
    public BusinessInfo Business { get; set; } = new();
}

public class HealthStatus
{
    public bool IsRunning { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public List<PrinterInfo> ConfiguredPrinters { get; set; } = new();
}

public class ConfigResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}
