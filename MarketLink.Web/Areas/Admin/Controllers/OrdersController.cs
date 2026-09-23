using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using MarketLink.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public OrdersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(string? status)
    {
        ViewData["Title"] = "Platform Orders";

        OrderStatus? filterStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
        {
            filterStatus = parsed;
        }

        var orders = await _unitOfWork.Repository<Order>().Query()
            .Include(o => o.Customer)
                .ThenInclude(c => c.User)
            .Include(o => o.Farmer)
            .Include(o => o.PickupSlot)
                .ThenInclude(ps => ps.Market)
            .Include(o => o.Items)
            .Where(o => !filterStatus.HasValue || o.Status == filterStatus.Value)
            .OrderByDescending(o => o.OrderedAt)
            .ToListAsync();

        ViewBag.CurrentStatus = status;
        ViewBag.TotalVolume = orders.Sum(o => o.TotalAmount);

        return View(orders);
    }
}
