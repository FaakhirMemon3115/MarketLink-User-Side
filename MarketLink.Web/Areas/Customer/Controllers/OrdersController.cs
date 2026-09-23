using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Enums;
using MarketLink.Core.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize(Roles = "Customer")]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly IUnitOfWork _unitOfWork;

    public OrdersController(IOrderService orderService, ICartService cartService, IUnitOfWork unitOfWork)
    {
        _orderService = orderService;
        _cartService = cartService;
        _unitOfWork = unitOfWork;
    }

    private async Task<int> GetCustomerIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            var customers = await _unitOfWork.Repository<MarketLink.Core.Entities.Customer>().FindAsync(c => c.UserId == userId);
            var customer = customers.FirstOrDefault();
            if (customer != null) return customer.Id;
        }
        return 1; // Demo fallback
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Orders";
        var customerId = await GetCustomerIdAsync();
        var orders = await _orderService.GetCustomerOrdersAsync(customerId);
        return View(orders);
    }

    public async Task<IActionResult> History(string? statusFilter, string? q)
    {
        ViewData["Title"] = "Order History";
        var customerId = await GetCustomerIdAsync();

        var query = _unitOfWork.Repository<Order>().Query()
            .Include(o => o.Farmer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.CustomerId == customerId);

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<OrderStatus>(statusFilter, true, out var parsedStatus))
        {
            query = query.Where(o => o.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim();
            query = query.Where(o => o.OrderNumber.Contains(search) || 
                                     o.Farmer.FarmName.Contains(search) ||
                                     o.Items.Any(i => i.ProductNameSnapshot.Contains(search)));
        }

        var orders = await query.OrderByDescending(o => o.OrderedAt).ToListAsync();

        ViewBag.StatusFilter = statusFilter;
        ViewBag.Query = q;
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _unitOfWork.Repository<Order>().Query()
            .Include(o => o.Farmer).ThenInclude(f => f.User)
            .Include(o => o.Market)
            .Include(o => o.PickupSlot)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        var customerId = await GetCustomerIdAsync();
        if (order == null || order.CustomerId != customerId) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder(int id)
    {
        var customerId = await GetCustomerIdAsync();
        var order = await _unitOfWork.Repository<Order>().Query()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId);

        if (order == null)
        {
            TempData["Error"] = "Order not found.";
            return RedirectToAction(nameof(History));
        }

        int addedCount = 0;
        foreach (var item in order.Items)
        {
            try
            {
                await _cartService.AddToCartAsync(customerId, item.ProductId, item.QuantityKg);
                addedCount++;
            }
            catch
            {
                // In case a product is unavailable, skip it
            }
        }

        if (addedCount > 0)
        {
            TempData["Success"] = $"Added {addedCount} item(s) from Order #{order.OrderNumber} to your basket!";
            return RedirectToAction("Index", "Cart");
        }
        else
        {
            TempData["Error"] = "Items from this order are currently out of stock or unavailable.";
            return RedirectToAction(nameof(History));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(int marketId, DateTime pickupTime)
    {
        try
        {
            var customerId = await GetCustomerIdAsync();
            var cartItems = await _cartService.GetCartAsync(customerId);
            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // Dummy pickup slot ID for demo
            var order = await _orderService.PlaceOrderAsync(customerId, cartItems, 1, "Cash on Pickup");
            await _cartService.ClearCartAsync(customerId);
            
            TempData["Success"] = $"Order #{order.Id} placed successfully!";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Cart");
        }
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _orderService.CancelOrderAsync(id, "Customer request", "Customer");
            TempData["Success"] = "Order cancelled successfully.";
        }
        catch(Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
