using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;

namespace MarketLink.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize(Roles = "Customer")]
public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private int GetCustomerId() => 1; // Demo fallback

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Shopping Cart";
        var items = await _cartService.GetCartAsync(GetCustomerId());
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequest req)
    {
        if (req == null || req.ProductId <= 0 || req.QuantityKg <= 0)
            return Json(new { success = false, message = "Invalid request." });

        try
        {
            await _cartService.AddToCartAsync(GetCustomerId(), req.ProductId, req.QuantityKg);
            var cart = await _cartService.GetCartAsync(GetCustomerId());
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
            var cart = await _cartService.GetCartAsync(GetCustomerId());
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
        await _cartService.ClearCartAsync(GetCustomerId());
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
