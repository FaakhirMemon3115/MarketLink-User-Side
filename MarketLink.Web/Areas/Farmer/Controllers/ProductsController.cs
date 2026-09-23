using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;

namespace MarketLink.Web.Areas.Farmer.Controllers;

[Area("Farmer")]
[Authorize(Roles = "Farmer")]
public class ProductsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    private int GetFarmerId()
    {
        var claim = User.FindFirst("FarmerId");
        if (claim != null && int.TryParse(claim.Value, out int id)) return id;
        return 1; // Fallback
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Products";
        int farmerId = GetFarmerId();
        var products = await _unitOfWork.Products.FindAsync(p => p.FarmerId == farmerId);
        return View(products);
    }
}
