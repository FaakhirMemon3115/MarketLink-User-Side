using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ReviewsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Review Moderation";

        var reviews = await _unitOfWork.Repository<Review>().Query()
            .Include(r => r.Customer)
                .ThenInclude(c => c.User)
            .Include(r => r.Product)
            .Include(r => r.Farmer)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reviews);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var review = await _unitOfWork.Repository<Review>().GetByIdAsync(id);
        if (review != null)
        {
            review.IsApproved = true;
            await _unitOfWork.Repository<Review>().UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Review approved and visible to customers.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _unitOfWork.Repository<Review>().GetByIdAsync(id);
        if (review != null)
        {
            _unitOfWork.Repository<Review>().Remove(review);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Review deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}
