namespace PrintAgent.Service.Models;

// ============================================================================
// REQUEST PARA IMPRIMIR FACTURA
// ============================================================================

public class PrintBillRequest
{
    public string PrinterName { get; set; } = "factura";
    public BillData Bill { get; set; } = new();
    public BusinessInfo? Business { get; set; }
}

/// <summary>
/// Coincide con BillDTO + BillItemDTO[] del sistema nico
/// </summary>
public class BillData
{
    public string Id { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
    public string DeliveryMethod { get; set; } = "DINE_IN"; // DINE_IN | TAKEOUT | DELIVERY
    public int? TableNumber { get; set; }
    public string? RoomName { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerIdNumber { get; set; }
    public string? DeliveryAddress { get; set; }

    // Totales
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public decimal? Tip { get; set; }

    // Pago (campos legacy para compatibilidad)
    public string? PayMethod { get; set; }
    public decimal? AmountPaid { get; set; }
    public decimal? Change { get; set; }

    // Pagos múltiples (nuevo)
    public List<BillPaymentData>? Payments { get; set; }

    // Fechas (ISO string desde JS)
    public string? CreatedAt { get; set; }
    public string? ClosedAt { get; set; }

    // Items
    public List<BillItemData> Items { get; set; } = new();

    // Notas adicionales
    public string? Notes { get; set; }

    // Indica si es una reimpresión
    public bool IsReprint { get; set; }
}

/// <summary>
/// Información de pago individual
/// </summary>
public class BillPaymentData
{
    public string PayMethodName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? AmountReceived { get; set; }
    public decimal? Change { get; set; }
}

/// <summary>
/// Coincide con BillItemDTO del sistema nico
/// </summary>
public class BillItemData
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public bool IsCancelled { get; set; }

    // Modificadores del item (nuevo)
    public List<BillItemModifierGroup>? Modifiers { get; set; }
}

/// <summary>
/// Grupo de modificadores para un item de factura
/// </summary>
public class BillItemModifierGroup
{
    public string GroupName { get; set; } = string.Empty;
    public List<BillItemModifierElement> Elements { get; set; } = new();
}

/// <summary>
/// Elemento individual de modificador
/// </summary>
public class BillItemModifierElement
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public bool IsCombined { get; set; }
}

// ============================================================================
// REQUEST PARA IMPRIMIR COMANDA DE COCINA
// ============================================================================

public class PrintKitchenRequest
{
    public string PrinterName { get; set; } = "cocina";
    public KitchenOrderData Order { get; set; } = new();
}

/// <summary>
/// Coincide con KitchenOrder del sistema nico
/// </summary>
public class KitchenOrderData
{
    public string KitchenMessageId { get; set; } = string.Empty;
    public int OrderNumber { get; set; } // Número de ronda/comanda
    public string BillId { get; set; } = string.Empty;
    public int BillOrderNumber { get; set; } // Número de la cuenta
    public int? TableNumber { get; set; }
    public string? RoomName { get; set; }
    public string DeliveryMethod { get; set; } = "DINE_IN";
    public string? CustomerName { get; set; }
    public List<KitchenOrderItemData> Items { get; set; } = new();
    public int WaitMinutes { get; set; }
    public string Status { get; set; } = "SENT_TO_KITCHEN"; // SENT_TO_KITCHEN | IN_PREPARATION | READY
    public string? InitTime { get; set; } // ISO string
    public string? Notes { get; set; }
}

/// <summary>
/// Item de comanda con artículos y modificadores
/// </summary>
public class KitchenOrderItemData
{
    public string BillItemId { get; set; } = string.Empty;
    public string SaleItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public List<KitchenArticleData> Articles { get; set; } = new();
}

/// <summary>
/// Artículo individual (unidad) con sus modificadores
/// </summary>
public class KitchenArticleData
{
    public string Description { get; set; } = string.Empty;
    public int ItemNumber { get; set; }
    public List<KitchenModifierGroupData>? Modifiers { get; set; }
}

/// <summary>
/// Grupo de modificadores (ej: "Término", "Extras")
/// </summary>
public class KitchenModifierGroupData
{
    public string Name { get; set; } = string.Empty;
    public bool ShowLabel { get; set; }
    public List<KitchenModifierElementData> Elements { get; set; } = new();
}

/// <summary>
/// Elemento de modificador (ej: "Término medio", "Sin cebolla")
/// </summary>
public class KitchenModifierElementData
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public bool IsCombined { get; set; }
}

// ============================================================================
// REQUEST PARA PRE-CUENTA
// ============================================================================

public class PrintPreBillRequest
{
    public string PrinterName { get; set; } = "factura";
    public BillData Bill { get; set; } = new();
    public BusinessInfo? Business { get; set; }
}
