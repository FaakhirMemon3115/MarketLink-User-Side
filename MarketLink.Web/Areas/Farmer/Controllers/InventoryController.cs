using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketLink.Web.Areas.Farmer.Controllers;

[Area("Farmer")]
[Authorize(Roles = "Farmer")]
public class InventoryController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Weekly Inventory Management";
        int farmerId = await GetFarmerIdAsync();

        var products = await _unitOfWork.Repository<Product>().Query()
            .Include(p => p.Category)
            .Where(p => p.FarmerId == farmerId)
            .OrderBy(p => p.StockQuantityKg)
            .ToListAsync();

        ViewBag.LowStockCount = products.Count(p => p.StockQuantityKg < 10);
        ViewBag.OutOfStockCount = products.Count(p => p.StockQuantityKg == 0 || !p.IsAvailable);

        return View(products);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStock(int id, int stockQuantityKg, bool isAvailable)
    {
        int farmerId = await GetFarmerIdAsync();
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

        if (product != null && product.FarmerId == farmerId)
        {
            product.StockQuantityKg = Math.Max(0, stockQuantityKg);
            product.IsAvailable = isAvailable && product.StockQuantityKg > 0;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Product>().UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = $"Stock updated for {product.Name}!";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickRestock(int id, int addQuantity)
    {
        int farmerId = await GetFarmerIdAsync();
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

        if (product != null && product.FarmerId == farmerId)
        {
            product.StockQuantityKg += Math.Max(1, addQuantity);
            product.IsAvailable = true;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Product>().UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = $"Added +{addQuantity} {product.Unit} to {product.Name}!";
        }

        return RedirectToAction(nameof(Index));
    }
}
