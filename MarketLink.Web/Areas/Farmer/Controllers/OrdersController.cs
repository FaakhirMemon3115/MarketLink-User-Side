using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using MarketLink.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketLink.Web.Areas.Farmer.Controllers;

[Area("Farmer")]
[Authorize(Roles = "Farmer")]
public class OrdersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderService _orderService;
    private readonly INotificationService _notificationService;

    public OrdersController(IUnitOfWork unitOfWork, IOrderService orderService, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _orderService = orderService;
        _notificationService = notificationService;
    }

    private async Task<int> GetFarmerIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            var farmer = await _unitOfWork.Repository<MarketLink.Core.Entities.Farmer>()
                .FirstOrDefaultAsync(f => f.UserId == userId);
            if (farmer != null) return farmer.Id;
        }
        return 1;
    }

    public async Task<IActionResult> Index(string? status)
    {
        ViewData["Title"] = "Customer Orders";
        int farmerId = await GetFarmerIdAsync();

        OrderStatus? filterStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
        {
            filterStatus = parsed;
        }

        var orders = await _unitOfWork.Repository<Order>().Query()
            .Include(o => o.Customer)
                .ThenInclude(c => c.User)
            .Include(o => o.PickupSlot)
                .ThenInclude(ps => ps.Market)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.FarmerId == farmerId)
            .Where(o => !filterStatus.HasValue || o.Status == filterStatus.Value)
            .OrderByDescending(o => o.OrderedAt)
            .ToListAsync();

        ViewBag.CurrentStatus = status;
        ViewBag.PendingCount = await _unitOfWork.Repository<Order>()
            .CountAsync(o => o.FarmerId == farmerId && o.Status == OrderStatus.Pending);

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        int farmerId = await GetFarmerIdAsync();

        var order = await _unitOfWork.Repository<Order>().Query()
            .Include(o => o.Customer)
                .ThenInclude(c => c.User)
            .Include(o => o.PickupSlot)
                .ThenInclude(ps => ps.Market)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == farmerId);

        if (order == null) return NotFound();

        ViewData["Title"] = $"Order #{order.OrderNumber}";
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id)
    {
        int farmerId = await GetFarmerIdAsync();
        var success = await _orderService.AcceptOrderAsync(id, farmerId);

        if (success)
        {
            var order = await _unitOfWork.Repository<Order>().Query()
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                await _notificationService.SendAsync(
                    order.Customer.UserId,
                    NotificationType.OrderAccepted,
                    "Order Accepted! 🥕",
                    $"Your order #{order.OrderNumber} has been accepted by the farmer.",
                    $"/Customer/Orders/Details/{order.Id}");
            }
            TempData["Success"] = "Order marked as Accepted.";
        }
        else
        {
            TempData["Error"] = "Unable to accept order.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkReady(int id)
    {
        var success = await _orderService.UpdateOrderStatusAsync(id, OrderStatus.ReadyForPickup, "Order prepared and packed.");

        if (success)
        {
            var order = await _unitOfWork.Repository<Order>().Query()
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                await _notificationService.SendAsync(
                    order.Customer.UserId,
                    NotificationType.OrderReady,
                    "Order Ready for Pickup! 🧺",
                    $"Your order #{order.OrderNumber} is fresh, packed, and ready at the market stall.",
                    $"/Customer/Orders/Details/{order.Id}");
            }
            TempData["Success"] = "Order marked as Ready for Pickup.";
        }
        else
        {
            TempData["Error"] = "Unable to update order status.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var success = await _orderService.UpdateOrderStatusAsync(id, OrderStatus.Completed, "Picked up and paid physically.");

        if (success)
        {
            var order = await _unitOfWork.Repository<Order>().Query()
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                await _notificationService.SendAsync(
                    order.Customer.UserId,
                    NotificationType.OrderDelivered,
                    "Order Completed! 🌟",
                    $"Thank you for collecting order #{order.OrderNumber}. Enjoy your fresh farm produce!",
                    $"/Customer/Orders/Details/{order.Id}");
            }
            TempData["Success"] = "Order marked as Completed (Payment received at pickup).";
        }
        else
        {
            TempData["Error"] = "Unable to complete order.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reason)
    {
        var success = await _orderService.CancelOrderAsync(id, reason ?? "Unavailable", "Farmer");

        if (success)
        {
            var order = await _unitOfWork.Repository<Order>().Query()
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                await _notificationService.SendAsync(
                    order.Customer.UserId,
                    NotificationType.OrderCancelled,
                    "Order Cancelled",
                    $"Your order #{order.OrderNumber} was cancelled by the farmer. Reason: {reason ?? "Stock unavailable"}.",
                    $"/Customer/Orders/Details/{order.Id}");
            }
            TempData["Success"] = "Order cancelled.";
        }
        else
        {
            TempData["Error"] = "Unable to cancel order.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
