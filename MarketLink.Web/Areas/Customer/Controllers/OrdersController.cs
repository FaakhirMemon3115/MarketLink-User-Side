using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Enums;
using System.Security.Claims;

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

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Orders";
        var orders = await _orderService.GetCustomerOrdersAsync(GetUserId());
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id);
        if (order == null || order.CustomerId != GetUserId()) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(int marketId, DateTime pickupTime)
    {
        try
        {
            var cart = await _cartService.GetCartAsync(GetUserId());
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var order = await _orderService.CreateOrderAsync(GetUserId(), marketId, pickupTime, "Cash on Pickup");
            await _cartService.ClearCartAsync(GetUserId());
            
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
            await _orderService.UpdateOrderStatusAsync(id, OrderStatus.Cancelled);
            TempData["Success"] = "Order cancelled successfully.";
        }
        catch(Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
