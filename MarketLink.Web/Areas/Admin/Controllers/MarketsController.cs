using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MarketsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public MarketsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Market Management";
        var markets = await _unitOfWork.Repository<Market>().Query()
            .Include(m => m.FarmerMarkets)
            .OrderBy(m => m.Name)
            .ToListAsync();

        return View(markets);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Add New Market";
        return View(new Market { OperatingHours = "Saturdays 8:00 AM - 1:00 PM", OpenDays = "Saturday, Sunday", IsActive = true, Latitude = 37.7749, Longitude = -122.4194 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Market model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError("Name", "Market name is required.");

        if (string.IsNullOrWhiteSpace(model.Address))
            ModelState.AddModelError("Address", "Address is required.");

        if (!ModelState.IsValid)
            return View(model);

        model.CreatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Market>().AddAsync(model);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = $"Market '{model.Name}' created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var market = await _unitOfWork.Repository<Market>().GetByIdAsync(id);
        if (market == null) return NotFound();

        ViewData["Title"] = $"Edit {market.Name}";
        return View(market);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Market model)
    {
        var existing = await _unitOfWork.Repository<Market>().GetByIdAsync(model.Id);
        if (existing == null) return NotFound();

        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError("Name", "Market name is required.");

        if (!ModelState.IsValid)
            return View(model);

        existing.Name = model.Name.Trim();
        existing.Description = model.Description?.Trim();
        existing.Address = model.Address.Trim();
        existing.City = model.City.Trim();
        existing.State = model.State.Trim();
        existing.Latitude = model.Latitude;
        existing.Longitude = model.Longitude;
        existing.OperatingHours = model.OperatingHours?.Trim();
        existing.OpenDays = model.OpenDays?.Trim();
        existing.Phone = model.Phone?.Trim();
        existing.Email = model.Email?.Trim();
        existing.Website = model.Website?.Trim();
        existing.ImageUrl = model.ImageUrl?.Trim();
        existing.IsActive = model.IsActive;
        existing.IsFeatured = model.IsFeatured;

        await _unitOfWork.Repository<Market>().UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = $"Market '{existing.Name}' updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var market = await _unitOfWork.Repository<Market>().GetByIdAsync(id);
        if (market != null)
        {
            _unitOfWork.Repository<Market>().Remove(market);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Market removed successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}
