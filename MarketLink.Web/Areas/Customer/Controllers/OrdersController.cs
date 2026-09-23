using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Enums;

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

    private int GetCustomerId() => 1; // Demo fallback

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Orders";
        var orders = await _orderService.GetCustomerOrdersAsync(GetCustomerId());
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _unitOfWork.Repository<MarketLink.Core.Entities.Order>().GetByIdAsync(id);
        if (order == null || order.CustomerId != GetCustomerId()) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(int marketId, DateTime pickupTime)
    {
        try
        {
            var cartItems = await _cartService.GetCartAsync(GetCustomerId());
            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // Dummy pickup slot ID for demo
            var order = await _orderService.PlaceOrderAsync(GetCustomerId(), cartItems, 1, "Cash on Pickup");
            await _cartService.ClearCartAsync(GetCustomerId());
            
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
