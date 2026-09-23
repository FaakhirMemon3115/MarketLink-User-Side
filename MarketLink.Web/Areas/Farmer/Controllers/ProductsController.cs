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
public class ProductsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductService _productService;

    public ProductsController(IUnitOfWork unitOfWork, IProductService productService)
    {
        _unitOfWork = unitOfWork;
        _productService = productService;
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
        return 1; // Fallback for seed demo
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Produce Catalog";
        int farmerId = await GetFarmerIdAsync();

        var products = await _unitOfWork.Repository<Product>().Query()
            .Include(p => p.Category)
            .Where(p => p.FarmerId == farmerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Add New Produce";
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
        ViewBag.Categories = categories.OrderBy(c => c.Name).ToList();
        return View(new Product { Unit = "KG", StockQuantityKg = 50, PricePerKg = 3.50m, IsAvailable = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model, string? imageUrl)
    {
        int farmerId = await GetFarmerIdAsync();
        model.FarmerId = farmerId;

        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError("Name", "Produce name is required.");

        if (model.PricePerKg <= 0)
            ModelState.AddModelError("PricePerKg", "Price must be greater than zero.");

        if (!ModelState.IsValid)
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            ViewBag.Categories = categories.OrderBy(c => c.Name).ToList();
            return View(model);
        }

        model.ImageUrl = imageUrl?.Trim();
        var images = !string.IsNullOrEmpty(imageUrl) ? new[] { imageUrl.Trim() } : Array.Empty<string>();

        await _productService.CreateAsync(model, images);
        TempData["Success"] = $"Produce '{model.Name}' added successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        int farmerId = await GetFarmerIdAsync();
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

        if (product == null || product.FarmerId != farmerId)
            return NotFound();

        ViewData["Title"] = $"Edit {product.Name}";
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
        ViewBag.Categories = categories.OrderBy(c => c.Name).ToList();

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product model, string? imageUrl)
    {
        int farmerId = await GetFarmerIdAsync();
        var existing = await _unitOfWork.Repository<Product>().GetByIdAsync(model.Id);

        if (existing == null || existing.FarmerId != farmerId)
            return NotFound();

        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError("Name", "Produce name is required.");

        if (model.PricePerKg <= 0)
            ModelState.AddModelError("PricePerKg", "Price must be greater than zero.");

        if (!ModelState.IsValid)
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            ViewBag.Categories = categories.OrderBy(c => c.Name).ToList();
            return View(model);
        }

        existing.Name = model.Name.Trim();
        existing.CategoryId = model.CategoryId;
        existing.Description = model.Description?.Trim();
        existing.PricePerKg = model.PricePerKg;
        existing.StockQuantityKg = model.StockQuantityKg;
        existing.Unit = model.Unit;
        existing.IsOrganic = model.IsOrganic;
        existing.IsAvailable = model.IsAvailable;
        existing.Season = model.Season;
        if (!string.IsNullOrEmpty(imageUrl)) existing.ImageUrl = imageUrl.Trim();

        await _unitOfWork.Repository<Product>().UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Produce updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        int farmerId = await GetFarmerIdAsync();
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

        if (product != null && product.FarmerId == farmerId)
        {
            _unitOfWork.Repository<Product>().Remove(product);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Produce deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}
