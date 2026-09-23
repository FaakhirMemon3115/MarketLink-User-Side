using Microsoft.AspNetCore.Mvc;
using MarketLink.Core.Interfaces;

namespace MarketLink.Web.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(IProductService productService, IUnitOfWork unitOfWork)
    {
        _productService = productService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetActiveProductsAsync();
        var categories = await _unitOfWork.Categories.GetAllAsync();
        var markets = await _unitOfWork.Markets.GetAllAsync();

        ViewBag.Categories = categories;
        ViewBag.Markets = markets;
        
        return View(products.Take(8).ToList());
    }

    public IActionResult About() => View();
    
    public IActionResult Contact() => View();
    
    public IActionResult FAQ() => View();

    public IActionResult Privacy() => View();
    
    public IActionResult Terms() => View();
}
