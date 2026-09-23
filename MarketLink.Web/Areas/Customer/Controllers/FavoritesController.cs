using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize(Roles = "Customer")]
public class FavoritesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public FavoritesController(IUnitOfWork unitOfWork)
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
        return 1; // Demo fallback
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Favorites";
        int customerId = await GetCustomerIdAsync();

        var favorites = await _unitOfWork.Repository<Favorite>().Query()
            .Include(f => f.Product).ThenInclude(p => p.Category)
            .Include(f => f.Product).ThenInclude(p => p.Farmer)
            .Include(f => f.Product).ThenInclude(p => p.Images)
            .Include(f => f.Farmer).ThenInclude(farmer => farmer.User)
            .Where(f => f.CustomerId == customerId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return View(favorites);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(string type, int id)
    {
        if (string.IsNullOrEmpty(type) || id <= 0)
        {
            return Json(new { success = false, message = "Invalid parameters." });
        }

        try
        {
            int customerId = await GetCustomerIdAsync();
            var favRepo = _unitOfWork.Repository<Favorite>();

            Favorite? existing = null;
            if (type.ToLower() == "product")
            {
                var favs = await favRepo.FindAsync(f => f.CustomerId == customerId && f.ProductId == id);
                existing = favs.FirstOrDefault();
            }
            else if (type.ToLower() == "farmer")
            {
                var favs = await favRepo.FindAsync(f => f.CustomerId == customerId && f.FarmerId == id);
                existing = favs.FirstOrDefault();
            }
            else
            {
                return Json(new { success = false, message = "Invalid type." });
            }

            bool isFavorite;
            if (existing != null)
            {
                favRepo.Remove(existing);
                isFavorite = false;
            }
            else
            {
                var newFav = new Favorite
                {
                    CustomerId = customerId,
                    CreatedAt = DateTime.UtcNow
                };

                if (type.ToLower() == "product") newFav.ProductId = id;
                if (type.ToLower() == "farmer") newFav.FarmerId = id;

                await favRepo.AddAsync(newFav);
                isFavorite = true;
            }

            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, isFavorite });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        int customerId = await GetCustomerIdAsync();
        var favRepo = _unitOfWork.Repository<Favorite>();
        var fav = await favRepo.GetByIdAsync(id);

        if (fav != null && fav.CustomerId == customerId)
        {
            favRepo.Remove(fav);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Item removed from favorites.";
        }

        return RedirectToAction(nameof(Index));
    }
}
