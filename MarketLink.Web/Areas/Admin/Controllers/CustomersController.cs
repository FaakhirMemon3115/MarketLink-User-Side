using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using CustomerEntity = MarketLink.Core.Entities.Customer;

namespace MarketLink.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CustomersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Customer Management";

        var customers = await _unitOfWork.Repository<CustomerEntity>().Query()
            .Include(c => c.User)
            .Include(c => c.Orders)
            .Include(c => c.Favorites)
            .OrderByDescending(c => c.User.CreatedAt)
            .ToListAsync();

        return View(customers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var customer = await _unitOfWork.Repository<CustomerEntity>().Query()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer?.User != null)
        {
            customer.User.IsActive = !customer.User.IsActive;
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = $"Customer account {(customer.User.IsActive ? "activated" : "deactivated")}.";
        }

        return RedirectToAction(nameof(Index));
    }
}
