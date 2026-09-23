using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.ViewModels;
using MarketLink.Core.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize(Roles = "Customer")]
public class DashboardController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;

    public DashboardController(IUnitOfWork unitOfWork, IProductService productService, IOrderService orderService)
    {
        _unitOfWork = unitOfWork;
        _productService = productService;
        _orderService = orderService;
    }

    private async Task<(int CustomerId, string CustomerName, string? City)> GetCustomerInfoAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            var customer = await _unitOfWork.Repository<MarketLink.Core.Entities.Customer>().Query()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer != null)
            {
                var name = $"{customer.User?.FirstName} {customer.User?.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(name)) name = User.Identity?.Name ?? "Customer";
                return (customer.Id, name, customer.DefaultCity);
            }
        }
        return (1, User.Identity?.Name ?? "Customer", "Local Area");
    }

    public async Task<IActionResult> Index()
    {
        var (custId, custName, city) = await GetCustomerInfoAsync();

        // 1. Fetch Featured / Fresh products directly for the Dashboard's Browse section
        var products = await _unitOfWork.Repository<Product>().Query()
            .Include(p => p.Category)
            .Include(p => p.Farmer)
            .Include(p => p.Images)
            .Where(p => p.IsAvailable && p.Farmer.Status == MarketLink.Core.Enums.FarmerStatus.Approved)
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.Id)
            .Take(8)
            .ToListAsync();

        // 2. Fetch Customer Favorites
        var favorites = await _unitOfWork.Repository<Favorite>().Query()
            .Include(f => f.Product).ThenInclude(p => p.Category)
            .Include(f => f.Product).ThenInclude(p => p.Farmer)
            .Include(f => f.Farmer)
            .Where(f => f.CustomerId == custId)
            .OrderByDescending(f => f.CreatedAt)
            .Take(6)
            .ToListAsync();

        var favProductIds = favorites.Where(f => f.ProductId.HasValue).Select(f => f.ProductId!.Value).ToHashSet();

        // 3. Fetch Customer Orders
        var recentOrders = await _unitOfWork.Repository<Order>().Query()
            .Include(o => o.Farmer)
            .Include(o => o.Items)
            .Where(o => o.CustomerId == custId)
            .OrderByDescending(o => o.OrderedAt)
            .Take(5)
            .ToListAsync();

        var allOrders = await _unitOfWork.Repository<Order>().FindAsync(o => o.CustomerId == custId);
        var addresses = await _unitOfWork.Repository<CustomerAddress>().FindAsync(a => a.CustomerId == custId);
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync();

        var totalSpent = allOrders.Where(o => o.Status != MarketLink.Core.Enums.OrderStatus.Cancelled).Sum(o => o.TotalAmount);

        var model = new DashboardViewModel
        {
            CustomerName = custName,
            DefaultCity = city,
            TotalOrdersCount = allOrders.Count(),
            TotalFavoritesCount = favorites.Count,
            TotalAddressesCount = addresses.Count(),
            TotalSpent = totalSpent,
            Products = products,
            Categories = categories.Take(6),
            Favorites = favorites,
            RecentOrders = recentOrders,
            FavoriteProductIds = favProductIds
        };

        return View(model);
    }

    public IActionResult Settings() => RedirectToAction("Index", "Profile");
}
