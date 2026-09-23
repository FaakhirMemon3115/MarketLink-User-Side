using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketLink.Web.Areas.Farmer.Controllers;

[Area("Farmer")]
[Authorize(Roles = "Farmer")]
public class ReviewsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private async Task<int> GetFarmerIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            var farmer = await _unitOfWork.Repository<MarketLink.Core.Entities.Farmer>()
                .FirstOrDefaultAsync(f => f.UserId == userId);
            if (farmer != null) return farmer.Id;
        }
        return 1;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Customer Reviews";
        int farmerId = await GetFarmerIdAsync();

        var reviews = await _unitOfWork.Repository<Review>().Query()
            .Include(r => r.Customer)
                .ThenInclude(c => c.User)
            .Include(r => r.Product)
            .Where(r => r.FarmerId == farmerId || (r.Product != null && r.Product.FarmerId == farmerId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 5.0;
        ViewBag.ReviewCount = reviews.Count;

        return View(reviews);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int id, string farmerResponse)
    {
        int farmerId = await GetFarmerIdAsync();

        var review = await _unitOfWork.Repository<Review>().Query()
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id && (r.FarmerId == farmerId || (r.Product != null && r.Product.FarmerId == farmerId)));

        if (review != null)
        {
            review.FarmerResponse = farmerResponse?.Trim();
            review.FarmerRespondedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Review>().UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Response saved and published!";
        }
        else
        {
            TempData["Error"] = "Review not found.";
        }

        return RedirectToAction(nameof(Index));
    }
}
