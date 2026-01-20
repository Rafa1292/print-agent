using System.Drawing.Printing;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrintAgent.Service.Models;

namespace PrintAgent.Service.Services;

/// <summary>
/// Servicio de impresión que envía comandos ESC/POS directamente a impresoras térmicas
/// </summary>
public class PrinterService : IPrinterService
{
    private readonly ILogger<PrinterService> _logger;
    private readonly PrintAgentSettings _settings;
    private readonly TicketBuilder _ticketBuilder;

    public PrinterService(
        ILogger<PrinterService> logger,
        IOptions<PrintAgentSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        _ticketBuilder = new TicketBuilder(_settings.Printers.FirstOrDefault()?.PaperWidth ?? 48);
    }

    public PrintResult PrintBill(PrintBillRequest request)
    {
        try
        {
            var printer = GetPrinterConfig(request.PrinterName);
            if (printer == null)
            {
                return PrintResult.Error($"Impresora '{request.PrinterName}' no configurada", "PRINTER_NOT_FOUND");
            }

            var ticketBuilder = new TicketBuilder(printer.PaperWidth);
            var ticketData = ticketBuilder.BuildBillTicket(request.Bill, request.Business ?? _settings.Business);

            var result = SendRawDataToPrinter(printer.SystemName, ticketData);
            if (!result)
            {
                return PrintResult.Error("Error al enviar datos a la impresora", "PRINT_ERROR");
            }

            _logger.LogInformation("Factura #{OrderNumber} impresa en {Printer}",
                request.Bill.OrderNumber, printer.SystemName);

            return PrintResult.Ok($"Factura #{request.Bill.OrderNumber} impresa correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir factura");
            return PrintResult.Error($"Error: {ex.Message}", "EXCEPTION");
        }
    }

    public PrintResult PrintPreBill(PrintPreBillRequest request)
    {
        try
        {
            var printer = GetPrinterConfig(request.PrinterName);
            if (printer == null)
            {
                return PrintResult.Error($"Impresora '{request.PrinterName}' no configurada", "PRINTER_NOT_FOUND");
            }

            var ticketBuilder = new TicketBuilder(printer.PaperWidth);
            var ticketData = ticketBuilder.BuildPreBillTicket(request.Bill, request.Business ?? _settings.Business);

            var result = SendRawDataToPrinter(printer.SystemName, ticketData);
            if (!result)
            {
                return PrintResult.Error("Error al enviar datos a la impresora", "PRINT_ERROR");
            }

            _logger.LogInformation("Pre-cuenta #{OrderNumber} impresa en {Printer}",
                request.Bill.OrderNumber, printer.SystemName);

            return PrintResult.Ok($"Pre-cuenta #{request.Bill.OrderNumber} impresa correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir pre-cuenta");
            return PrintResult.Error($"Error: {ex.Message}", "EXCEPTION");
        }
    }

    public PrintResult PrintKitchenOrder(PrintKitchenRequest request)
    {
        try
        {
            var printer = GetPrinterConfig(request.PrinterName);
            if (printer == null)
            {
                return PrintResult.Error($"Impresora '{request.PrinterName}' no configurada", "PRINTER_NOT_FOUND");
            }

            var ticketBuilder = new TicketBuilder(printer.PaperWidth);
            var ticketData = ticketBuilder.BuildKitchenTicket(request.Order);

            var result = SendRawDataToPrinter(printer.SystemName, ticketData);
            if (!result)
            {
                return PrintResult.Error("Error al enviar datos a la impresora", "PRINT_ERROR");
            }

            _logger.LogInformation("Comanda #{OrderNumber} (Cuenta #{BillOrder}) impresa en {Printer}",
                request.Order.OrderNumber, request.Order.BillOrderNumber, printer.SystemName);

            return PrintResult.Ok($"Comanda impresa correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir comanda");
            return PrintResult.Error($"Error: {ex.Message}", "EXCEPTION");
        }
    }

    public bool TestPrinter(string printerName)
    {
        try
        {
            var printer = GetPrinterConfig(printerName);
            if (printer == null)
            {
                _logger.LogWarning("Impresora '{PrinterName}' no encontrada para prueba", printerName);
                return false;
            }

            var ticketBuilder = new TicketBuilder(printer.PaperWidth);
            var testData = ticketBuilder.BuildTestPage(printer.Name, _settings.Business);

            var result = SendRawDataToPrinter(printer.SystemName, testData);

            if (result)
            {
                _logger.LogInformation("Prueba de impresión exitosa en {Printer}", printer.SystemName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en prueba de impresión");
            return false;
        }
    }

    public List<PrinterInfo> GetConfiguredPrinters()
    {
        var systemPrinters = GetSystemPrinters();

        return _settings.Printers.Select(p => new PrinterInfo
        {
            Name = p.Name,
            SystemName = p.SystemName,
            PaperWidth = p.PaperWidth,
            IsDefault = p.IsDefault,
            Type = p.Type,
            IsOnline = systemPrinters.Contains(p.SystemName, StringComparer.OrdinalIgnoreCase)
        }).ToList();
    }

    public List<string> GetSystemPrinters()
    {
        var printers = new List<string>();

        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            printers.Add(printer);
        }

        return printers;
    }

    private PrinterConfig? GetPrinterConfig(string name)
    {
        return _settings.Printers.FirstOrDefault(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    #region RAW Printing via Windows API

    /// <summary>
    /// Envía datos RAW directamente a la impresora usando Windows API
    /// </summary>
    private bool SendRawDataToPrinter(string printerName, byte[] data)
    {
        var ptrData = IntPtr.Zero;
        var di = new DOCINFOA
        {
            pDocName = "PrintAgent Ticket",
            pDataType = "RAW"
        };

        if (!OpenPrinter(printerName.Normalize(), out var hPrinter, IntPtr.Zero))
        {
            _logger.LogError("No se pudo abrir la impresora: {Printer}. Error: {Error}",
                printerName, Marshal.GetLastWin32Error());
            return false;
        }

        try
        {
            if (!StartDocPrinter(hPrinter, 1, di))
            {
                _logger.LogError("No se pudo iniciar el documento. Error: {Error}",
                    Marshal.GetLastWin32Error());
                return false;
            }

            try
            {
                if (!StartPagePrinter(hPrinter))
                {
                    _logger.LogError("No se pudo iniciar la página. Error: {Error}",
                        Marshal.GetLastWin32Error());
                    return false;
                }

                try
                {
                    ptrData = Marshal.AllocCoTaskMem(data.Length);
                    Marshal.Copy(data, 0, ptrData, data.Length);

                    if (!WritePrinter(hPrinter, ptrData, data.Length, out var written))
                    {
                        _logger.LogError("No se pudo escribir a la impresora. Error: {Error}",
                            Marshal.GetLastWin32Error());
                        return false;
                    }

                    if (written != data.Length)
                    {
                        _logger.LogWarning("Se escribieron {Written} de {Total} bytes",
                            written, data.Length);
                    }

                    return true;
                }
                finally
                {
                    EndPagePrinter(hPrinter);
                    if (ptrData != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(ptrData);
                    }
                }
            }
            finally
            {
                EndDocPrinter(hPrinter);
            }
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }

    #region P/Invoke declarations

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private class DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pDocName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pDataType;
    }

    [DllImport("winspool.drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern bool OpenPrinter(
        [MarshalAs(UnmanagedType.LPStr)] string szPrinter,
        out IntPtr hPrinter,
        IntPtr pd);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern bool StartDocPrinter(
        IntPtr hPrinter,
        int level,
        [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(
        IntPtr hPrinter,
        IntPtr pBytes,
        int dwCount,
        out int dwWritten);

    #endregion

    #endregion
}
