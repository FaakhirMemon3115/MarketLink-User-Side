using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using MarketLink.Core.Enums;
using Microsoft.EntityFrameworkCore;
using FarmerEntity = MarketLink.Core.Entities.Farmer;
using CustomerEntity = MarketLink.Core.Entities.Customer;

namespace MarketLink.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Admin Overview";
        var farmers  = await _unitOfWork.Repository<FarmerEntity>().GetAllAsync();
        var customers = await _unitOfWork.Repository<CustomerEntity>().GetAllAsync();
        var products  = await _unitOfWork.Repository<Product>().GetAllAsync();
        var markets   = await _unitOfWork.Repository<Market>().GetAllAsync();
        var orders    = await _unitOfWork.Repository<Order>().GetAllAsync();

        ViewBag.FarmerCount   = farmers.Count();
        ViewBag.CustomerCount = customers.Count();
        ViewBag.ProductCount  = products.Count();
        ViewBag.MarketCount   = markets.Count();
        ViewBag.OrderCount    = orders.Count();
        ViewBag.TotalRevenue  = orders
            .Where(o => o.Status == OrderStatus.Completed)
            .Sum(o => o.TotalAmount);
        ViewBag.PendingFarmers = farmers.Count(f => f.Status == FarmerStatus.Pending);

        return View();
    }

    public async Task<IActionResult> Notifications()
    {
        ViewData["Title"] = "Notification Broadcast Center";
        var notifications = await _unitOfWork.Repository<Notification>().Query()
            .Include(n => n.User)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Broadcast(string targetRole, string title, string message)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
        {
            TempData["Error"] = "Title and message are required.";
            return RedirectToAction(nameof(Notifications));
        }

        // Use UserManager to query Identity users properly
        var allUsers = _userManager.Users.Where(u => u.IsActive).ToList();

        IEnumerable<ApplicationUser> targets = allUsers;
        if (!string.IsNullOrEmpty(targetRole) && targetRole != "All")
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(targetRole);
            targets = usersInRole.Where(u => u.IsActive);
        }

        int sentCount = 0;
        foreach (var u in targets)
        {
            await _notificationService.SendAsync(
                u.Id,
                NotificationType.General,
                title.Trim(),
                message.Trim(),
                "/Home/Index");
            sentCount++;
        }

        TempData["Success"] = $"Broadcast delivered to {sentCount} active users!";
        return RedirectToAction(nameof(Notifications));
    }

    public async Task<IActionResult> Settings()
    {
        ViewData["Title"] = "System & Marketplace Settings";
        var settings = await _unitOfWork.Repository<SiteSetting>().GetAllAsync();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSetting(string key, string value, string? description)
    {
        var setting = await _unitOfWork.Repository<SiteSetting>()
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting == null)
        {
            setting = new SiteSetting
            {
                Key = key,
                Value = value,
                Description = description,
                Group = "General"
            };
            await _unitOfWork.Repository<SiteSetting>().AddAsync(setting);
        }
        else
        {
            setting.Value = value;
            if (!string.IsNullOrEmpty(description)) setting.Description = description;
            _unitOfWork.Repository<SiteSetting>().Update(setting);
        }

        await _unitOfWork.SaveChangesAsync();
        TempData["Success"] = $"Setting '{key}' saved successfully!";
        return RedirectToAction(nameof(Settings));
    }
}
