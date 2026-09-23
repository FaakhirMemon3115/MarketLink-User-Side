using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize(Roles = "Customer")]
public class ReviewsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
        return 1;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Reviews";
        int customerId = await GetCustomerIdAsync();

        var reviews = await _unitOfWork.Repository<Review>().Query()
            .Include(r => r.Product)
            .Include(r => r.Farmer)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reviews);
    }
}
