using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

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

    private async Task<int> GetCustomerIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            var customers = await _unitOfWork.Repository<MarketLink.Core.Entities.Customer>().FindAsync(c => c.UserId == userId);
            var customer = customers.FirstOrDefault();
            if (customer != null) return customer.Id;
        }
        return 1; // fallback for demo
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel();
        model.Products = await _productService.GetFeaturedAsync(8);
        var custId = await GetCustomerIdAsync();
        var favRepo = _unitOfWork.Repository<MarketLink.Core.Entities.Favorite>();
        model.Favorites = await favRepo.FindAsync(f => f.CustomerId == custId);
        model.RecentOrders = await _orderService.GetCustomerOrdersAsync(custId);
        return View(model);
    }
}
