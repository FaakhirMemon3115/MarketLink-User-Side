using MarketLink.Core.Entities;
using MarketLink.Core.Enums;
using MarketLink.Core.Interfaces;
using MarketLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notificationService;

    public OrderService(ApplicationDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<Order> PlaceOrderAsync(int customerId, IEnumerable<CartItem> items, int pickupSlotId, string? notes)
    {
        var cartList = items.ToList();
        if (!cartList.Any()) throw new InvalidOperationException("Cart is empty");

        // Group by farmer (one order per farmer)
        var farmerGroups = cartList.GroupBy(i => i.Product.FarmerId);
        Order? firstOrder = null;

        foreach (var group in farmerGroups)
        {
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                CustomerId = customerId,
                FarmerId = group.Key,
                PickupSlotId = pickupSlotId,
                Notes = notes,
                Status = OrderStatus.Pending,
                OrderedAt = DateTime.UtcNow
            };

            decimal subtotal = 0;
            foreach (var cartItem in group)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    QuantityKg = cartItem.QuantityKg,
                    PricePerKgSnapshot = cartItem.Product.PricePerKg,
                    TotalPrice = cartItem.Product.PricePerKg * cartItem.QuantityKg,
                    ProductNameSnapshot = cartItem.Product.Name
                };
                order.Items.Add(orderItem);
                subtotal += orderItem.TotalPrice;

                // Reduce stock
                cartItem.Product.StockQuantityKg -= cartItem.QuantityKg;
                _db.Products.Update(cartItem.Product);
            }

            order.SubTotal = subtotal;
            order.TotalAmount = subtotal;
            _db.Orders.Add(order);
            firstOrder ??= order;
        }

        await _db.SaveChangesAsync();

        // Clear cart
        _db.CartItems.RemoveRange(cartList);
        await _db.SaveChangesAsync();

        return firstOrder!;
    }

    public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        => await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.Farmer).ThenInclude(f => f.User)
            .Include(o => o.PickupSlot)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

    public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(int customerId)
        => await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(o => o.Farmer).ThenInclude(f => f.User)
            .Include(o => o.PickupSlot)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderedAt)
            .ToListAsync();

    public async Task<IEnumerable<Order>> GetFarmerOrdersAsync(int farmerId, OrderStatus? status = null)
    {
        var q = _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.PickupSlot)
            .Where(o => o.FarmerId == farmerId);

        if (status.HasValue) q = q.Where(o => o.Status == status.Value);
        return await q.OrderByDescending(o => o.OrderedAt).ToListAsync();
    }

    public async Task<bool> AcceptOrderAsync(int orderId, int farmerId)
    {
        var order = await _db.Orders.Include(o => o.Customer).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.FarmerId == farmerId);
        if (order == null || order.Status != OrderStatus.Pending) return false;

        order.Status = OrderStatus.Accepted;
        order.AcceptedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _notificationService.SendAsync(
            order.Customer.UserId, NotificationType.OrderAccepted,
            "Order Accepted!", $"Your order #{order.OrderNumber} has been accepted by the farmer.",
            $"/Customer/Orders/Details/{order.OrderNumber}");

        return true;
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? notes = null)
    {
        var order = await _db.Orders
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return false;

        order.Status = newStatus;
        if (notes != null) order.FarmerNotes = notes;

        if (newStatus == OrderStatus.ReadyForPickup) order.ReadyAt = DateTime.UtcNow;
        if (newStatus == OrderStatus.Completed) order.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        if (newStatus == OrderStatus.ReadyForPickup)
            await _notificationService.SendAsync(
                order.Customer.UserId, NotificationType.OrderReady,
                "Order Ready!", $"Your order #{order.OrderNumber} is ready for pickup!",
                $"/Customer/Orders/Details/{order.OrderNumber}");

        return true;
    }

    public async Task<bool> CancelOrderAsync(int orderId, string reason, string cancelledBy)
    {
        var order = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.Farmer).ThenInclude(f => f.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return false;

        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = reason;
        order.CancelledAt = DateTime.UtcNow;

        // Restore stock
        foreach (var item in order.Items)
        {
            item.Product.StockQuantityKg += item.QuantityKg;
            _db.Products.Update(item.Product);
        }

        await _db.SaveChangesAsync();

        await _notificationService.SendAsync(
            order.Customer.UserId, NotificationType.OrderCancelled,
            "Order Cancelled", $"Your order #{order.OrderNumber} has been cancelled. Reason: {reason}",
            $"/Customer/Orders/Details/{order.OrderNumber}");

        return true;
    }

    private static string GenerateOrderNumber()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()[^6..];
        var random = Random.Shared.Next(1000, 9999);
        return $"ML-{timestamp}-{random}";
    }
}
