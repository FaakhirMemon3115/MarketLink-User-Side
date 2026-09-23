using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;

namespace MarketLink.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Admin Dashboard";
        var farmers = await _unitOfWork.Farmers.GetAllAsync();
        var customers = await _unitOfWork.Customers.GetAllAsync();
        var products = await _unitOfWork.Products.GetAllAsync();
        
        ViewBag.FarmerCount = farmers.Count();
        ViewBag.CustomerCount = customers.Count();
        ViewBag.ProductCount = products.Count();
        
        return View();
    }
}
