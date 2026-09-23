using MarketLink.Core.Enums;

namespace MarketLink.Core.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int FarmerId { get; set; }
    public int? PickupSlotId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal SubTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? FarmerNotes { get; set; }
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public Farmer Farmer { get; set; } = null!;
    public PickupSlot? PickupSlot { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int QuantityKg { get; set; }
    public decimal PricePerKgSnapshot { get; set; }
    public decimal TotalPrice { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;

    // Navigation
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
