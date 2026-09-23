using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketLink.Web.Areas.Farmer.Controllers;

[Area("Farmer")]
[Authorize(Roles = "Farmer")]
public class PickupSlotsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public PickupSlotsController(IUnitOfWork unitOfWork)
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
        ViewData["Title"] = "Pickup Slots";
        int farmerId = await GetFarmerIdAsync();

        var slots = await _unitOfWork.Repository<PickupSlot>().Query()
            .Include(ps => ps.Market)
            .Where(ps => ps.FarmerId == farmerId)
            .OrderBy(ps => ps.DayOfWeek)
            .ThenBy(ps => ps.StartTime)
            .ToListAsync();

        var markets = await _unitOfWork.Repository<Market>().GetAllAsync();
        ViewBag.Markets = markets.OrderBy(m => m.Name).ToList();

        return View(slots);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int marketId, DayOfWeek dayOfWeek, string startTime, string endTime, int maxOrders = 20, string? notes = null)
    {
        int farmerId = await GetFarmerIdAsync();

        if (TimeOnly.TryParse(startTime, out var start) && TimeOnly.TryParse(endTime, out var end))
        {
            var slot = new PickupSlot
            {
                FarmerId = farmerId,
                MarketId = marketId,
                DayOfWeek = dayOfWeek,
                StartTime = start,
                EndTime = end,
                MaxOrders = maxOrders > 0 ? maxOrders : 20,
                IsActive = true,
                Notes = notes?.Trim()
            };

            await _unitOfWork.Repository<PickupSlot>().AddAsync(slot);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "New pickup window added successfully!";
        }
        else
        {
            TempData["Error"] = "Invalid time format provided.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        int farmerId = await GetFarmerIdAsync();
        var slot = await _unitOfWork.Repository<PickupSlot>().GetByIdAsync(id);

        if (slot != null && slot.FarmerId == farmerId)
        {
            slot.IsActive = !slot.IsActive;
            await _unitOfWork.Repository<PickupSlot>().UpdateAsync(slot);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = slot.IsActive ? "Slot enabled." : "Slot disabled.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        int farmerId = await GetFarmerIdAsync();
        var slot = await _unitOfWork.Repository<PickupSlot>().GetByIdAsync(id);

        if (slot != null && slot.FarmerId == farmerId)
        {
            _unitOfWork.Repository<PickupSlot>().Remove(slot);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Pickup slot removed.";
        }

        return RedirectToAction(nameof(Index));
    }
}
