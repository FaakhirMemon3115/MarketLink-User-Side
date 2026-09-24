using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using System.Security.Claims;

namespace MarketLink.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IUnitOfWork _unitOfWork;

    public CartController(ICartService cartService, IUnitOfWork unitOfWork)
    {
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
        if (!User.IsInRole("Customer"))
        {
            return RedirectToAction("AccessDenied", "Auth", new { area = "" });
        }
        ViewData["Title"] = "Shopping Cart";
        var customerId = await GetCustomerIdAsync();
        var items = await _cartService.GetCartAsync(customerId);
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequest req)
    {
        if (User.IsInRole("Admin") || User.IsInRole("Farmer"))
        {
            return Json(new { success = false, message = "You are not eligible for this function." });
        }

        if (req == null || req.ProductId <= 0 || req.QuantityKg <= 0)
            return Json(new { success = false, message = "Invalid request." });

        try
        {
            var customerId = await GetCustomerIdAsync();
            await _cartService.AddToCartAsync(customerId, req.ProductId, req.QuantityKg);
            var cart = await _cartService.GetCartAsync(customerId);
            return Json(new { success = true, cartCount = cart.Count() });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem([FromBody] RemoveFromCartRequest req)
    {
        try
        {
            // Note: RemoveFromCartAsync takes cartItemId, but we just pass productId for demo simplicity. 
            // Better to fetch cart item by productId first.
            var customerId = await GetCustomerIdAsync();
            var cart = await _cartService.GetCartAsync(customerId);
            var item = cart.FirstOrDefault(c => c.ProductId == req.ProductId);
            if (item != null)
                await _cartService.RemoveFromCartAsync(item.Id);
            return Json(new { success = true });
        }
        catch
        {
            return Json(new { success = false });
        }
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        var customerId = await GetCustomerIdAsync();
        await _cartService.ClearCartAsync(customerId);
        return RedirectToAction(nameof(Index));
    }
}

public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int QuantityKg { get; set; }
}

public class RemoveFromCartRequest
{
    public int ProductId { get; set; }
}
