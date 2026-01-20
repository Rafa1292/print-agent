using PrintAgent.Service.Models;

namespace PrintAgent.Service.Services;

/// <summary>
/// Construye tickets de impresión usando comandos ESC/POS
/// </summary>
public class TicketBuilder
{
    private readonly int _lineWidth;

    public TicketBuilder(int lineWidth = 48)
    {
        _lineWidth = lineWidth;
    }

    /// <summary>
    /// Construye ticket de factura/cuenta
    /// </summary>
    public byte[] BuildBillTicket(BillData bill, BusinessInfo? business, byte[]? logoBitmap = null)
    {
        var builder = new EscPosBuilder(_lineWidth);

        // Slogan de reimpresión si aplica
        if (bill.IsReprint)
        {
            builder
                .AlignCenter()
                .Bold()
                .Line("*** REIMPRESION ***")
                .Bold(false)
                .Lines(1);
        }

        // Logo del negocio (si está disponible)
        if (logoBitmap != null && logoBitmap.Length > 0)
        {
            builder.AlignCenter();
            builder.RawBytes(logoBitmap);
            builder.Lines(1);
        }

        // Encabezado del negocio
        if (business != null)
        {
            builder
                .AlignCenter()
                .Bold()
                .DoubleSize()
                .Line(business.Name)
                .NormalSize()
                .Bold(false);

            if (!string.IsNullOrEmpty(business.Address))
            {
                builder.Line(business.Address);
            }
            if (!string.IsNullOrEmpty(business.Address2))
            {
                builder.Line(business.Address2);
            }
            if (!string.IsNullOrEmpty(business.Phone))
            {
                builder.Line($"Tel: {business.Phone}");
            }
            if (!string.IsNullOrEmpty(business.TaxId))
            {
                builder.Line($"Ced: {business.TaxId}");
            }
            builder.Lines(1);
        }

        // Tipo de documento y número
        builder
            .AlignCenter()
            .Bold()
            .DoubleHeight()
            .Line($"CUENTA #{bill.OrderNumber}")
            .NormalSize()
            .Bold(false);

        // Información de entrega
        builder.AlignLeft();

        if (bill.DeliveryMethod == "DINE_IN" && bill.TableNumber.HasValue)
        {
            builder.Line($"Mesa: {bill.TableNumber}");
        }
        else if (bill.DeliveryMethod == "TAKEOUT")
        {
            builder.Line("** PARA LLEVAR **");
        }
        else if (bill.DeliveryMethod == "DELIVERY")
        {
            builder.Line("** DELIVERY **");
            if (!string.IsNullOrEmpty(bill.DeliveryAddress))
            {
                builder.Line($"Dir: {bill.DeliveryAddress}");
            }
        }

        // Cliente
        if (!string.IsNullOrEmpty(bill.CustomerName))
        {
            builder.Line($"Cliente: {bill.CustomerName}");
            if (!string.IsNullOrEmpty(bill.CustomerIdNumber))
            {
                builder.Line($"Doc: {bill.CustomerIdNumber}");
            }
            if (!string.IsNullOrEmpty(bill.CustomerPhone))
            {
                builder.Line($"Tel: {bill.CustomerPhone}");
            }
        }

        // Fecha
        var date = ParseDate(bill.CreatedAt) ?? DateTime.Now;
        builder.Line($"Fecha: {date:dd/MM/yyyy HH:mm}");

        // Dirección del cliente (después de la fecha)
        if (!string.IsNullOrEmpty(bill.CustomerAddress))
        {
            builder.WrappedLine(bill.CustomerAddress);
        }

        // Datos de facturación (solo si están presentes)
        if (!string.IsNullOrEmpty(bill.InvoiceIdNumber))
        {
            builder.Line($"Cedula: {bill.InvoiceIdNumber}");
        }
        if (!string.IsNullOrEmpty(bill.InvoiceCommercialName))
        {
            builder.WrappedLine(bill.InvoiceCommercialName);
        }
        if (!string.IsNullOrEmpty(bill.InvoiceEmail))
        {
            builder.Line(bill.InvoiceEmail);
        }

        builder.Separator();

        // Encabezado de items
        builder
            .Bold()
            .Row("CANT", "DESCRIPCION", "TOTAL")
            .Bold(false)
            .Separator();

        // Items
        foreach (var item in bill.Items.Where(i => !i.IsCancelled))
        {
            // Primera línea: cantidad y descripción
            string qtyStr = item.Quantity.ToString("0.##");
            string totalStr = item.Total.ToString("0.00");

            // Calcular espacio disponible para descripción
            int qtyWidth = 5;
            int totalWidth = 10;
            int descWidth = _lineWidth - qtyWidth - totalWidth;

            string desc = item.Description;
            if (desc.Length > descWidth)
            {
                desc = desc[..(descWidth - 2)] + "..";
            }

            // Total del item en negrita para diferenciarlo
            builder
                .Bold()
                .Line($"{qtyStr.PadRight(qtyWidth)}{desc.PadRight(descWidth)}{totalStr.PadLeft(totalWidth)}")
                .Bold(false);

            // Mostrar modificadores si existen
            if (item.Modifiers != null && item.Modifiers.Count > 0)
            {
                foreach (var modGroup in item.Modifiers)
                {
                    foreach (var element in modGroup.Elements)
                    {
                        // Si es combinado, mostrar etiqueta
                        if (element.IsCombined)
                        {
                            builder.Line("    Combinado con:");
                        }

                        string modLine = element.Quantity > 1
                            ? $"  + {element.Quantity}x {element.Name}"
                            : $"  + {element.Name}";

                        // Mostrar precio total (precio × cantidad) si es mayor a 0
                        if (element.Price > 0)
                        {
                            decimal totalPrice = element.Price * element.Quantity;
                            string priceStr = totalPrice.ToString("0.00");
                            int availableWidth = _lineWidth - priceStr.Length - 1;
                            if (modLine.Length > availableWidth)
                            {
                                modLine = modLine[..(availableWidth - 2)] + "..";
                            }
                            builder.Line($"{modLine.PadRight(availableWidth)} {priceStr}");
                        }
                        else
                        {
                            builder.Line(modLine);
                        }
                    }
                }
            }

            // Si hay descuento en el item, mostrarlo
            if (item.Discount > 0)
            {
                builder.Line($"     Desc: -{item.Discount:0.00}");
            }
        }

        builder.Separator();

        // Totales
        builder
            .Row("Subtotal:", bill.Subtotal.ToString("0.00"))
            .Row("IVA:", bill.Tax.ToString("0.00"));

        if (bill.Discount > 0)
        {
            builder.Row("Descuento:", $"-{bill.Discount:0.00}");
        }

        if (bill.Tip.HasValue && bill.Tip.Value > 0)
        {
            builder.Row("Propina:", bill.Tip.Value.ToString("0.00"));
        }

        builder
            .DoubleSeparator()
            .Bold()
            .DoubleHeight()
            .Row("TOTAL:", bill.Total.ToString("0.00"))
            .NormalSize()
            .Bold(false);

        // Métodos de pago
        builder.Lines(1);

        // Primero verificar si hay pagos múltiples
        if (bill.Payments != null && bill.Payments.Count > 0)
        {
            foreach (var payment in bill.Payments)
            {
                builder.Row($"{payment.PayMethodName}:", payment.Amount.ToString("0.00"));

                // Si es efectivo, mostrar recibido y cambio
                if (payment.AmountReceived.HasValue && payment.AmountReceived.Value > payment.Amount)
                {
                    builder.Row("  Recibido:", payment.AmountReceived.Value.ToString("0.00"));
                    if (payment.Change.HasValue && payment.Change.Value > 0)
                    {
                        builder.Row("  Cambio:", payment.Change.Value.ToString("0.00"));
                    }
                }
            }
        }
        // Fallback a campos legacy si no hay pagos múltiples
        else if (!string.IsNullOrEmpty(bill.PayMethod))
        {
            builder.Row("Pago:", bill.PayMethod);

            if (bill.AmountPaid.HasValue)
            {
                builder.Row("Recibido:", bill.AmountPaid.Value.ToString("0.00"));
            }

            if (bill.Change.HasValue && bill.Change.Value > 0)
            {
                builder.Row("Cambio:", bill.Change.Value.ToString("0.00"));
            }
        }

        // Notas
        if (!string.IsNullOrEmpty(bill.Notes))
        {
            builder
                .Lines(1)
                .Line($"Nota: {bill.Notes}");
        }

        // Pie de ticket con mensaje personalizado
        builder
            .Lines(2)
            .AlignCenter();

        if (business != null && !string.IsNullOrEmpty(business.ThankYouMessage))
        {
            builder.Line(business.ThankYouMessage);
        }
        else
        {
            builder.Line("Gracias por su preferencia!");
        }

        // Leyenda legal al final (Régimen Simplificado, etc.)
        if (business != null && !string.IsNullOrEmpty(business.LegalDisclaimer))
        {
            builder
                .Lines(1)
                .Line(business.LegalDisclaimer);
        }

        builder
            .Lines(1)
            .Cut();

        return builder.Build();
    }

    /// <summary>
    /// Construye ticket de pre-cuenta (sin método de pago)
    /// </summary>
    public byte[] BuildPreBillTicket(BillData bill, BusinessInfo? business, byte[]? logoBitmap = null)
    {
        var builder = new EscPosBuilder(_lineWidth);

        // Logo del negocio (si está disponible)
        if (logoBitmap != null && logoBitmap.Length > 0)
        {
            builder.AlignCenter();
            builder.RawBytes(logoBitmap);
            builder.Lines(1);
        }

        // Encabezado
        builder
            .AlignCenter()
            .Bold()
            .DoubleSize()
            .Line("*** PRE-CUENTA ***")
            .NormalSize()
            .Bold(false)
            .Lines(1);

        // Info del negocio (más compacta)
        if (business != null)
        {
            builder.Line(business.Name);
        }

        // Número de cuenta y mesa
        builder
            .DoubleHeight()
            .Line($"Cuenta #{bill.OrderNumber}")
            .NormalSize();

        if (bill.TableNumber.HasValue)
        {
            builder.Line($"Mesa: {bill.TableNumber}");
        }

        builder.AlignLeft();

        // Fecha
        var date = ParseDate(bill.CreatedAt) ?? DateTime.Now;
        builder.Line($"Fecha: {date:dd/MM/yyyy HH:mm}");

        builder.Separator();

        // Items (formato compacto)
        foreach (var item in bill.Items.Where(i => !i.IsCancelled))
        {
            // Item con total en negrita
            builder
                .Bold()
                .ItemLine(item.Description, item.Quantity, item.UnitPrice, item.Total)
                .Bold(false);

            // Mostrar modificadores si existen
            if (item.Modifiers != null && item.Modifiers.Count > 0)
            {
                foreach (var modGroup in item.Modifiers)
                {
                    foreach (var element in modGroup.Elements)
                    {
                        // Si es combinado, mostrar etiqueta
                        if (element.IsCombined)
                        {
                            builder.Line("    Combinado con:");
                        }

                        string modLine = element.Quantity > 1
                            ? $"  + {element.Quantity}x {element.Name}"
                            : $"  + {element.Name}";

                        // Mostrar precio total (precio × cantidad)
                        if (element.Price > 0)
                        {
                            decimal totalPrice = element.Price * element.Quantity;
                            builder.Line($"{modLine} ({totalPrice:0.00})");
                        }
                        else
                        {
                            builder.Line(modLine);
                        }
                    }
                }
            }
        }

        builder.Separator();

        // Totales
        builder
            .Row("Subtotal:", bill.Subtotal.ToString("0.00"))
            .Row("IVA:", bill.Tax.ToString("0.00"));

        if (bill.Discount > 0)
        {
            builder.Row("Descuento:", $"-{bill.Discount:0.00}");
        }

        builder
            .DoubleSeparator()
            .Bold()
            .DoubleHeight()
            .Row("TOTAL:", bill.Total.ToString("0.00"))
            .NormalSize()
            .Bold(false);

        // Aviso
        builder
            .Lines(2)
            .AlignCenter()
            .Line("Este NO es un comprobante fiscal")
            .Lines(1)
            .Cut();

        return builder.Build();
    }

    /// <summary>
    /// Construye ticket de comanda para cocina
    /// </summary>
    public byte[] BuildKitchenTicket(KitchenOrderData order)
    {
        var builder = new EscPosBuilder(_lineWidth);

        // Encabezado grande y visible
        builder
            .AlignCenter()
            .Bold()
            .DoubleSize();

        // Tipo de servicio
        switch (order.DeliveryMethod)
        {
            case "DINE_IN":
                if (order.TableNumber.HasValue)
                {
                    builder.Line($"MESA {order.TableNumber}");
                    if (!string.IsNullOrEmpty(order.RoomName))
                    {
                        builder.NormalSize().Line(order.RoomName).DoubleSize();
                    }
                }
                else
                {
                    builder.Line("MESA S/N");
                }
                break;
            case "TAKEOUT":
                builder.Line("PARA LLEVAR");
                break;
            case "DELIVERY":
                builder.Line("DELIVERY");
                break;
        }

        builder.NormalSize();

        // Número de cuenta y ronda
        builder
            .DoubleHeight()
            .Line($"Cuenta #{order.BillOrderNumber} - Ronda {order.OrderNumber}")
            .NormalSize()
            .Bold(false);

        // Cliente si existe
        if (!string.IsNullOrEmpty(order.CustomerName))
        {
            builder.Line($"Cliente: {order.CustomerName}");
        }

        // Hora
        var time = ParseDate(order.InitTime) ?? DateTime.Now;
        builder.Line($"Hora: {time:HH:mm}");

        builder.DoubleSeparator();

        // Items con artículos y modificadores
        foreach (var item in order.Items)
        {
            // Nombre del item con cantidad
            builder
                .Bold()
                .DoubleHeight()
                .Line($"{item.Quantity}x {item.SaleItemName}")
                .NormalSize()
                .Bold(false);

            // Artículos individuales (si hay más de uno o tienen modificadores)
            if (item.Articles.Count > 0)
            {
                foreach (var article in item.Articles)
                {
                    // Si la descripción es diferente al nombre del item, mostrarla
                    if (article.Description != item.SaleItemName)
                    {
                        builder.Line($"  #{article.ItemNumber}: {article.Description}");
                    }

                    // Modificadores
                    if (article.Modifiers != null)
                    {
                        foreach (var modGroup in article.Modifiers)
                        {
                            foreach (var element in modGroup.Elements)
                            {
                                string prefix = modGroup.ShowLabel ? $"{modGroup.Name}: " : "";
                                string qty = element.Quantity > 1 ? $"x{element.Quantity} " : "";
                                builder.Line($"    -> {prefix}{qty}{element.Name}");
                            }
                        }
                    }
                }
            }

            builder.Lines(1);
        }

        // Notas especiales
        if (!string.IsNullOrEmpty(order.Notes))
        {
            builder
                .DoubleSeparator()
                .Bold()
                .Line("NOTAS:")
                .Bold(false)
                .Line(order.Notes);
        }

        // Tiempo de espera si es relevante
        if (order.WaitMinutes > 5)
        {
            builder
                .Lines(1)
                .AlignCenter()
                .Bold()
                .Line($"** ESPERA: {order.WaitMinutes} min **")
                .Bold(false);
        }

        builder
            .Lines(2)
            .Cut();

        return builder.Build();
    }

    /// <summary>
    /// Construye página de prueba
    /// </summary>
    public byte[] BuildTestPage(string printerName, BusinessInfo? business)
    {
        var builder = new EscPosBuilder(_lineWidth);

        builder
            .AlignCenter()
            .Bold()
            .DoubleSize()
            .Line("PRUEBA DE IMPRESION")
            .NormalSize()
            .Bold(false)
            .Lines(1);

        if (business != null)
        {
            builder.Line(business.Name);
        }

        builder
            .Lines(1)
            .AlignLeft()
            .Line($"Impresora: {printerName}")
            .Line($"Ancho: {_lineWidth} caracteres")
            .Line($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
            .Lines(1)
            .Separator()
            .Line("Caracteres especiales:")
            .Line("aeiou AEIOU (sin acentos)")
            .Separator()
            .Lines(1)
            .Bold()
            .Line("Texto en negrita")
            .Bold(false)
            .DoubleHeight()
            .Line("Doble altura")
            .NormalSize()
            .DoubleWidth()
            .Line("Doble ancho")
            .NormalSize()
            .DoubleSize()
            .Line("Doble tamano")
            .NormalSize()
            .Lines(1)
            .AlignCenter()
            .Line("Impresion exitosa!")
            .Lines(1)
            .Cut();

        return builder.Build();
    }

    private static DateTime? ParseDate(string? isoDate)
    {
        if (string.IsNullOrEmpty(isoDate))
            return null;

        if (DateTime.TryParse(isoDate, out var date))
            return date;

        return null;
    }
}
