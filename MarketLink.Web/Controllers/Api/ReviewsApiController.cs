using Microsoft.AspNetCore.Mvc;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MarketLink.Web.Controllers.Api;

[ApiController]
[Route("api/reviews")]
[Produces("application/json")]
public class ReviewsApiController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewsApiController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetProductReviews(int productId)
    {
        var reviews = await _unitOfWork.Repository<Review>().Query()
            .Include(r => r.Customer).ThenInclude(c => c.User)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.Rating,
                r.Title,
                r.Comment,
                r.FarmerResponse,
                r.CreatedAt,
                CustomerName = $"{r.Customer.User.FirstName} {r.Customer.User.LastName}".Trim()
            })
            .ToListAsync();

        return Ok(new { success = true, count = reviews.Count, reviews });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitReview([FromBody] ApiSubmitReviewRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        var review = new Review
        {
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            FarmerId = request.FarmerId,
            Rating = request.Rating,
            Title = request.Title?.Trim(),
            Comment = request.Comment?.Trim(),
            IsApproved = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Review>().AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        return StatusCode(201, new { success = true, message = "Review submitted successfully.", reviewId = review.Id });
    }
}

public class ApiSubmitReviewRequest
{
    [Required]
    public int CustomerId { get; set; }
    public int? ProductId { get; set; }
    public int? FarmerId { get; set; }
    [Range(1, 5)]
    public int Rating { get; set; }
    public string? Title { get; set; }
    [Required]
    public string Comment { get; set; } = string.Empty;
}
