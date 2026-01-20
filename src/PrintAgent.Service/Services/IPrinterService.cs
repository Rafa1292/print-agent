using PrintAgent.Service.Models;

namespace PrintAgent.Service.Services;

public interface IPrinterService
{
    PrintResult PrintBill(PrintBillRequest request);
    PrintResult PrintKitchenOrder(PrintKitchenRequest request);
    PrintResult PrintPreBill(PrintPreBillRequest request);
    List<PrinterInfo> GetConfiguredPrinters();
    List<string> GetSystemPrinters();
    bool TestPrinter(string printerName);
}
