using Microsoft.AspNetCore.Mvc;
using MarketLink.Core.Interfaces;

namespace MarketLink.Web.Controllers;

public class HomeController : Controller
{
    private readonly MarketLink.Core.Interfaces.IProductService _productService;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(MarketLink.Core.Interfaces.IProductService productService, IUnitOfWork unitOfWork)
    {
        _productService = productService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetFeaturedAsync();
        var allCategories = await _unitOfWork.Repository<MarketLink.Core.Entities.Category>().GetAllAsync();
        var markets = await _unitOfWork.Repository<MarketLink.Core.Entities.Market>().GetAllAsync();

        // Main primary categories (or all if none marked)
        var mainCategories = allCategories.Where(c => c.ParentId == null).OrderBy(c => c.SortOrder).ToList();
        if (!mainCategories.Any()) mainCategories = allCategories.Take(6).ToList();

        ViewBag.Categories = mainCategories;
        ViewBag.AllCategories = allCategories;
        ViewBag.Markets = markets;
        
        return View(products.Take(8).ToList());
    }

    public IActionResult About() => View();
    
    public IActionResult Contact() => View();
    
    public IActionResult FAQ() => View();

    public IActionResult Privacy() => View();
    
    public IActionResult Terms() => View();
}
