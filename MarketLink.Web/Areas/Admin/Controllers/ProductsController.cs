using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using Microsoft.EntityFrameworkCore;
using FarmerEntity = MarketLink.Core.Entities.Farmer;

namespace MarketLink.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(int? categoryId, int? farmerId)
    {
        ViewData["Title"] = "Product Moderation";

        var query = _unitOfWork.Repository<Product>().Query()
            .Include(p => p.Category)
            .Include(p => p.Farmer)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (farmerId.HasValue)
            query = query.Where(p => p.FarmerId == farmerId.Value);

        var products = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

        var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
        var farmers = await _unitOfWork.Repository<FarmerEntity>().GetAllAsync();

        ViewBag.Categories = categories.OrderBy(c => c.Name).ToList();
        ViewBag.Farmers = farmers.OrderBy(f => f.FarmName).ToList();

        return View(products);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability(int id)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        if (product != null)
        {
            product.IsAvailable = !product.IsAvailable;
            await _unitOfWork.Repository<Product>().UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = $"Produce '{product.Name}' availability toggled.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        if (product != null)
        {
            _unitOfWork.Repository<Product>().Remove(product);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Product removed from marketplace.";
        }

        return RedirectToAction(nameof(Index));
    }
}
