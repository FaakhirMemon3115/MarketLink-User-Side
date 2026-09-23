using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Category Management";
        var categories = await _unitOfWork.Repository<Category>().Query()
            .Include(c => c.Products)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description, string? iconClass, int displayOrder = 0)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var slug = name.Trim().ToLower().Replace(" ", "-").Replace("&", "and");
            var cat = new Category
            {
                Name = name.Trim(),
                Slug = slug,
                Description = description?.Trim(),
                IconClass = iconClass?.Trim() ?? "fas fa-leaf",
                SortOrder = displayOrder,
                IsActive = true
            };

            await _unitOfWork.Repository<Category>().AddAsync(cat);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = $"Category '{name}' created!";
        }
        else
        {
            TempData["Error"] = "Category name cannot be empty.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string name, string? description, string? iconClass, int displayOrder, bool isActive)
    {
        var cat = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        if (cat != null)
        {
            cat.Name = name.Trim();
            cat.Description = description?.Trim();
            cat.IconClass = iconClass?.Trim() ?? "fas fa-leaf";
            cat.SortOrder = displayOrder;
            cat.IsActive = isActive;

            await _unitOfWork.Repository<Category>().UpdateAsync(cat);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Category updated successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await _unitOfWork.Repository<Category>().Query()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cat != null)
        {
            if (cat.Products.Any())
            {
                TempData["Error"] = $"Cannot delete category '{cat.Name}' because it contains {cat.Products.Count} products.";
            }
            else
            {
                _unitOfWork.Repository<Category>().Remove(cat);
                await _unitOfWork.SaveChangesAsync();
                TempData["Success"] = "Category deleted.";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
