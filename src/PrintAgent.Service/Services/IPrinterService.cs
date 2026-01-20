using PrintAgent.Service.Models;

namespace PrintAgent.Service.Services;

public interface IPrinterService
{
    PrintResult PrintBill(PrintBillRequest request);
    Task<PrintResult> PrintBillAsync(PrintBillRequest request, ILogoCacheService? logoCache);
    PrintResult PrintPreBill(PrintPreBillRequest request);
    Task<PrintResult> PrintPreBillAsync(PrintPreBillRequest request, ILogoCacheService? logoCache);
    PrintResult PrintKitchenOrder(PrintKitchenRequest request);
    List<PrinterInfo> GetConfiguredPrinters();
    List<string> GetSystemPrinters();
    bool TestPrinter(string printerName);
}
