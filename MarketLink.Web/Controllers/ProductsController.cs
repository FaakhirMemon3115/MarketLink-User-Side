using Microsoft.AspNetCore.Mvc;
using MarketLink.Core.Interfaces;

namespace MarketLink.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IProductService productService, IUnitOfWork unitOfWork)
    {
        _productService = productService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(string categorySlug, string q, bool? isOrganic, bool? seasonal, string sortBy, int page = 1)
    {
        var products = await _productService.GetActiveProductsAsync();

        if (!string.IsNullOrEmpty(categorySlug))
            products = products.Where(p => p.Category?.Slug == categorySlug);
            
        if (!string.IsNullOrEmpty(q))
            products = products.Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || 
                                           p.Description.Contains(q, StringComparison.OrdinalIgnoreCase));

        if (isOrganic == true)
            products = products.Where(p => p.IsOrganic);

        if (seasonal == true)
        {
            var currentMonth = DateTime.Now.Month;
            products = products.Where(p => _productService.IsProductInSeason(p, currentMonth));
        }

        products = sortBy switch
        {
            "price_asc" => products.OrderBy(p => p.PricePerKg),
            "price_desc" => products.OrderByDescending(p => p.PricePerKg),
            "name" => products.OrderBy(p => p.Name),
            _ => products.OrderByDescending(p => p.Id)
        };

        var categories = await _unitOfWork.Categories.GetAllAsync();
        ViewBag.Categories = categories;
        ViewBag.CurrentCategory = categorySlug;
        ViewBag.Query = q;
        ViewBag.IsOrganic = isOrganic;
        ViewBag.Seasonal = seasonal;
        ViewBag.SortBy = sortBy;

        return View(products.ToList());
    }

    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrEmpty(slug)) return NotFound();
        var product = await _productService.GetProductBySlugAsync(slug);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Autocomplete(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Json(new object[] {});
        var products = await _productService.GetActiveProductsAsync();
        var results = products
            .Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Take(6)
            .Select(p => new {
                p.Name,
                p.Slug,
                p.PricePerKg,
                p.ImageUrl,
                CategoryName = p.Category?.Name
            });
        return Json(results);
    }
}
