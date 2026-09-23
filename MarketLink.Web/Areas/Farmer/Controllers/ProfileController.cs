using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketLink.Web.Areas.Farmer.Controllers;

[Area("Farmer")]
[Authorize(Roles = "Farmer")]
public class ProfileController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProfileController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private async Task<MarketLink.Core.Entities.Farmer> GetCurrentFarmerAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            var farmer = await _unitOfWork.Repository<MarketLink.Core.Entities.Farmer>().Query()
                .Include(f => f.User)
                .Include(f => f.FarmerMarkets)
                    .ThenInclude(fm => fm.Market)
                .FirstOrDefaultAsync(f => f.UserId == userId);
            if (farmer != null) return farmer;
        }

        // Fallback for demo
        return await _unitOfWork.Repository<MarketLink.Core.Entities.Farmer>().Query()
            .Include(f => f.User)
            .Include(f => f.FarmerMarkets)
                .ThenInclude(fm => fm.Market)
            .FirstAsync();
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Farm Profile";
        var farmer = await GetCurrentFarmerAsync();
        return View(farmer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(MarketLink.Core.Entities.Farmer model)
    {
        var farmer = await GetCurrentFarmerAsync();

        farmer.FarmName = model.FarmName?.Trim() ?? farmer.FarmName;
        farmer.Description = model.Description?.Trim();
        farmer.Phone = model.Phone?.Trim();
        farmer.City = model.City?.Trim();
        farmer.State = model.State?.Trim();
        farmer.StallNumber = model.StallNumber?.Trim();
        farmer.Website = model.Website?.Trim();
        farmer.BannerImageUrl = model.BannerImageUrl?.Trim();
        farmer.ProfileImageUrl = model.ProfileImageUrl?.Trim();
        if (model.Latitude.HasValue) farmer.Latitude = model.Latitude;
        if (model.Longitude.HasValue) farmer.Longitude = model.Longitude;

        await _unitOfWork.Repository<MarketLink.Core.Entities.Farmer>().UpdateAsync(farmer);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Farm profile updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Markets()
    {
        ViewData["Title"] = "My Markets";
        var farmer = await GetCurrentFarmerAsync();
        var allMarkets = await _unitOfWork.Repository<Market>().GetAllAsync();

        ViewBag.AllMarkets = allMarkets.OrderBy(m => m.Name).ToList();
        return View(farmer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMarket(int marketId, string? stallNumber)
    {
        var farmer = await GetCurrentFarmerAsync();

        var existing = await _unitOfWork.Repository<FarmerMarket>()
            .FirstOrDefaultAsync(fm => fm.FarmerId == farmer.Id && fm.MarketId == marketId);

        if (existing == null)
        {
            var fm = new FarmerMarket
            {
                FarmerId = farmer.Id,
                MarketId = marketId,
                StallNumber = stallNumber?.Trim(),
                IsActive = true
            };
            await _unitOfWork.Repository<FarmerMarket>().AddAsync(fm);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Market associated with your farm!";
        }
        else
        {
            TempData["Error"] = "Market is already linked to your farm.";
        }

        return RedirectToAction(nameof(Markets));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMarket(int id)
    {
        var farmer = await GetCurrentFarmerAsync();
        var fm = await _unitOfWork.Repository<FarmerMarket>().GetByIdAsync(id);

        if (fm != null && fm.FarmerId == farmer.Id)
        {
            _unitOfWork.Repository<FarmerMarket>().Remove(fm);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Market removed from your profile.";
        }

        return RedirectToAction(nameof(Markets));
    }
}
