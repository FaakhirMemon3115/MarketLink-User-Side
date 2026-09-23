using Microsoft.AspNetCore.Mvc;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using MarketLink.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

    public async Task<IActionResult> Index(
        string? categorySlug,
        string? q,
        bool? isOrganic,
        bool? seasonal,
        decimal? minPrice,
        decimal? maxPrice,
        string? sortBy,
        int page = 1)
    {
        var productQuery = _unitOfWork.Repository<Product>().Query()
            .Include(p => p.Farmer).ThenInclude(f => f.User)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsAvailable && p.Farmer.Status == FarmerStatus.Approved);

        // Filter: Category
        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            productQuery = productQuery.Where(p => p.Category.Slug == categorySlug || (p.Category.Parent != null && p.Category.Parent.Slug == categorySlug));
        }

        // Filter: Search Keyword
        if (!string.IsNullOrWhiteSpace(q))
        {
            var searchTerm = q.Trim();
            productQuery = productQuery.Where(p =>
                p.Name.Contains(searchTerm) ||
                (p.Description != null && p.Description.Contains(searchTerm)) ||
                (p.Tags != null && p.Tags.Contains(searchTerm)) ||
                p.Category.Name.Contains(searchTerm) ||
                p.Farmer.FarmName.Contains(searchTerm));
        }

        // Filter: Organic
        if (isOrganic == true)
        {
            productQuery = productQuery.Where(p => p.IsOrganic);
        }

        // Filter: In Season Now
        if (seasonal == true)
        {
            var currentSeason = GetCurrentSeason();
            productQuery = productQuery.Where(p => p.Season == currentSeason || p.Season == Season.AllYear);
        }

        // Filter: Price Range
        if (minPrice.HasValue && minPrice.Value > 0)
        {
            productQuery = productQuery.Where(p => p.PricePerKg >= minPrice.Value);
        }

        if (maxPrice.HasValue && maxPrice.Value > 0)
        {
            productQuery = productQuery.Where(p => p.PricePerKg <= maxPrice.Value);
        }

        // Sorting
        productQuery = sortBy switch
        {
            "price_asc" => productQuery.OrderBy(p => p.PricePerKg),
            "price_desc" => productQuery.OrderByDescending(p => p.PricePerKg),
            "name" => productQuery.OrderBy(p => p.Name),
            "name_desc" => productQuery.OrderByDescending(p => p.Name),
            "bestseller" => productQuery.OrderByDescending(p => p.IsBestSeller).ThenByDescending(p => p.CreatedAt),
            "newest" => productQuery.OrderByDescending(p => p.CreatedAt),
            _ => productQuery.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.Id)
        };

        var totalCount = await productQuery.CountAsync();
        var products = await productQuery.ToListAsync();

        // Load Categories for filter sidebar
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
        ViewBag.Categories = categories.OrderBy(c => c.Name).ToList();

        // Customer's favorite products for instant heart toggle state
        var favoriteProductIds = new HashSet<int>();
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var customers = await _unitOfWork.Repository<Customer>().FindAsync(c => c.UserId == userId);
                var customer = customers.FirstOrDefault();
                if (customer != null)
                {
                    var favs = await _unitOfWork.Repository<Favorite>().FindAsync(f => f.CustomerId == customer.Id && f.ProductId.HasValue);
                    favoriteProductIds = favs.Select(f => f.ProductId!.Value).ToHashSet();
                }
            }
        }

        ViewBag.FavoriteProductIds = favoriteProductIds;
        ViewBag.CurrentCategory = categorySlug;
        ViewBag.Query = q;
        ViewBag.IsOrganic = isOrganic;
        ViewBag.Seasonal = seasonal;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;
        ViewBag.SortBy = sortBy;
        ViewBag.TotalCount = totalCount;

        return View(products);
    }

    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrEmpty(slug)) return NotFound();
        var product = await _productService.GetBySlugAsync(slug);
        if (product == null) return NotFound();

        bool isFav = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var customers = await _unitOfWork.Repository<Customer>().FindAsync(c => c.UserId == userId);
                var customer = customers.FirstOrDefault();
                if (customer != null)
                {
                    var favs = await _unitOfWork.Repository<Favorite>().FindAsync(f => f.CustomerId == customer.Id && f.ProductId == product.Id);
                    isFav = favs.Any();
                }
            }
        }
        ViewBag.IsFavorite = isFav;

        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Autocomplete(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Json(new object[] { });

        var searchTerm = q.Trim();
        var results = await _unitOfWork.Repository<Product>().Query()
            .Include(p => p.Category)
            .Where(p => p.IsAvailable && (p.Name.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm))))
            .Take(6)
            .Select(p => new
            {
                p.Name,
                p.Slug,
                p.PricePerKg,
                p.ImageUrl,
                CategoryName = p.Category != null ? p.Category.Name : "Produce"
            })
            .ToListAsync();

        return Json(results);
    }

    private static Season GetCurrentSeason()
    {
        var month = DateTime.UtcNow.Month;
        return month switch
        {
            3 or 4 or 5 => Season.Spring,
            6 or 7 or 8 => Season.Summer,
            9 or 10 or 11 => Season.Autumn,
            _ => Season.Winter
        };
    }
}
