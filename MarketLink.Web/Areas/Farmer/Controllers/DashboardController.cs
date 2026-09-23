using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Areas.Farmer.Controllers;

[Area("Farmer")]
[Authorize(Roles = "Farmer")]
public class DashboardController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderService _orderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(IUnitOfWork unitOfWork, IOrderService orderService, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _orderService = orderService;
        _userManager = userManager;
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

        var claim = User.FindFirst("FarmerId");
        if (claim != null && int.TryParse(claim.Value, out int id)) return id;
        return 1; // Fallback for demo
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Farmer Dashboard";
        int farmerId = await GetFarmerIdAsync();
        
        var products = await _unitOfWork.Repository<MarketLink.Core.Entities.Product>().FindAsync(p => p.FarmerId == farmerId);
        var orders = await _unitOfWork.Repository<Order>().Query()
            .Where(o => o.FarmerId == farmerId)
            .ToListAsync();

        ViewBag.ProductCount = products.Count();
        ViewBag.LowStockCount = products.Count(p => p.StockKg < 10);
        ViewBag.TotalOrdersCount = orders.Count;
        ViewBag.PendingOrdersCount = orders.Count(o => o.Status == MarketLink.Core.Enums.OrderStatus.Pending);
        
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        ViewData["Title"] = "Account Settings";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth", new { area = "" });

        var user = await _userManager.FindByIdAsync(userId);
        var farmer = await _unitOfWork.Repository<MarketLink.Core.Entities.Farmer>().Query()
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.UserId == userId);

        if (farmer == null)
        {
            farmer = await _unitOfWork.Repository<MarketLink.Core.Entities.Farmer>().Query()
                .Include(f => f.User)
                .FirstOrDefaultAsync() ?? new MarketLink.Core.Entities.Farmer();
        }

        ViewBag.User = user;
        return View(farmer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(string farmName, string description, string phone, string city, string state, string stallNumber)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth", new { area = "" });

        var farmer = await _unitOfWork.Repository<MarketLink.Core.Entities.Farmer>().Query()
            .FirstOrDefaultAsync(f => f.UserId == userId);

        if (farmer == null)
        {
            farmer = await _unitOfWork.Repository<MarketLink.Core.Entities.Farmer>().Query().FirstOrDefaultAsync();
        }

        if (farmer != null)
        {
            farmer.FarmName = farmName?.Trim() ?? farmer.FarmName;
            farmer.Description = description?.Trim();
            farmer.Phone = phone?.Trim();
            farmer.City = city?.Trim();
            farmer.State = state?.Trim();
            farmer.StallNumber = stallNumber?.Trim();

            _unitOfWork.Repository<MarketLink.Core.Entities.Farmer>().Update(farmer);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Farm settings updated successfully!";
        }

        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
        {
            TempData["Error"] = "New passwords do not match or are empty.";
            return RedirectToAction(nameof(Settings));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth", new { area = "" });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return RedirectToAction(nameof(Settings));

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded)
        {
            TempData["Success"] = "Password changed successfully!";
        }
        else
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Settings));
    }
}
