using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using System.Security.Claims;

namespace MarketLink.Web.Areas.Farmer.Controllers;

[Area("Farmer")]
[Authorize(Roles = "Farmer")]
public class DashboardController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderService _orderService;

    public DashboardController(IUnitOfWork unitOfWork, IOrderService orderService)
    {
        _unitOfWork = unitOfWork;
        _orderService = orderService;
    }

    private int GetFarmerId()
    {
        var claim = User.FindFirst("FarmerId");
        if (claim != null && int.TryParse(claim.Value, out int id)) return id;
        return 1; // Fallback for demo
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Farmer Dashboard";
        int farmerId = GetFarmerId();
        
        var products = await _unitOfWork.Products.FindAsync(p => p.FarmerId == farmerId);
        ViewBag.ProductCount = products.Count();
        ViewBag.LowStockCount = products.Count(p => p.StockKg < 5);
        
        return View();
    }
}
