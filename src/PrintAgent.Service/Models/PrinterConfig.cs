namespace PrintAgent.Service.Models;

public class PrinterConfig
{
    public string Name { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public int PaperWidth { get; set; } = 48; // Characters per line (default 80mm paper)
    public bool IsDefault { get; set; }
    public PrinterType Type { get; set; } = PrinterType.Receipt;
}

public enum PrinterType
{
    Receipt,
    Kitchen,
    Both
}

public class PrintAgentSettings
{
    public int Port { get; set; } = 5123;
    public List<PrinterConfig> Printers { get; set; } = new();
    public BusinessInfo Business { get; set; } = new();
}

public class BusinessInfo
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? Logo { get; set; } // URL del logo (para referencia, no se imprime en ESC/POS básico)
    public string? ThankYouMessage { get; set; } // Mensaje de agradecimiento al final del ticket
    public string? LegalDisclaimer { get; set; } // Leyenda legal (ej: "Régimen Simplificado")
}
