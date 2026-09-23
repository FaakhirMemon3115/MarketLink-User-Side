using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using System.Security.Claims;

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

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Shopping Cart";
        var cart = await _cartService.GetCartAsync(GetUserId());
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequest req)
    {
        if (req == null || req.ProductId <= 0 || req.QuantityKg <= 0)
            return Json(new { success = false, message = "Invalid request." });

        try
        {
            var cart = await _cartService.AddItemAsync(GetUserId(), req.ProductId, req.QuantityKg);
            return Json(new { success = true, cartCount = cart.Items.Count });
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
            await _cartService.RemoveItemAsync(GetUserId(), req.ProductId);
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
        await _cartService.ClearCartAsync(GetUserId());
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
