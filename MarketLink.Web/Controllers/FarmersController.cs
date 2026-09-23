using Microsoft.AspNetCore.Mvc;
using MarketLink.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Controllers;

public class FarmersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public FarmersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var farmers = await _unitOfWork.Farmers.GetAllAsync();
        return View(farmers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var farmer = await _unitOfWork.Farmers.GetByIdAsync(id);
        if (farmer == null) return NotFound();
        
        var products = await _unitOfWork.Products.FindAsync(p => p.FarmerId == id && p.IsActive);
        ViewBag.Products = products.ToList();
        
        return View(farmer);
    }
}
