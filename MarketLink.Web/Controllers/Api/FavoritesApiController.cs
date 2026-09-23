using Microsoft.AspNetCore.Mvc;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MarketLink.Web.Controllers.Api;

[ApiController]
[Route("api/favorites")]
[Produces("application/json")]
public class FavoritesApiController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public FavoritesApiController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("customer/{customerId:int}")]
    public async Task<IActionResult> GetFavorites(int customerId)
    {
        var favs = await _unitOfWork.Repository<Favorite>().Query()
            .Include(f => f.Product).ThenInclude(p => p.Category)
            .Include(f => f.Farmer)
            .Where(f => f.CustomerId == customerId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new
            {
                f.Id,
                f.ProductId,
                f.FarmerId,
                f.CreatedAt,
                Product = f.Product != null ? new
                {
                    f.Product.Id,
                    f.Product.Name,
                    f.Product.PricePerKg,
                    f.Product.ImageUrl,
                    CategoryName = f.Product.Category.Name
                } : null,
                Farmer = f.Farmer != null ? new
                {
                    f.Farmer.Id,
                    f.Farmer.FarmName,
                    f.Farmer.Bio
                } : null
            })
            .ToListAsync();

        return Ok(new { success = true, count = favs.Count, favorites = favs });
    }

    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleFavorite([FromBody] ApiToggleFavoriteRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        var repo = _unitOfWork.Repository<Favorite>();
        Favorite? existing = null;

        if (request.Type.ToLower() == "product")
        {
            var results = await repo.FindAsync(f => f.CustomerId == request.CustomerId && f.ProductId == request.Id);
            existing = results.FirstOrDefault();
        }
        else if (request.Type.ToLower() == "farmer")
        {
            var results = await repo.FindAsync(f => f.CustomerId == request.CustomerId && f.FarmerId == request.Id);
            existing = results.FirstOrDefault();
        }
        else
        {
            return BadRequest(new { success = false, message = "Type must be either 'product' or 'farmer'." });
        }

        bool isFavorite;
        if (existing != null)
        {
            repo.Remove(existing);
            isFavorite = false;
        }
        else
        {
            var newFav = new Favorite
            {
                CustomerId = request.CustomerId,
                CreatedAt = DateTime.UtcNow
            };
            if (request.Type.ToLower() == "product") newFav.ProductId = request.Id;
            if (request.Type.ToLower() == "farmer") newFav.FarmerId = request.Id;

            await repo.AddAsync(newFav);
            isFavorite = true;
        }

        await _unitOfWork.SaveChangesAsync();
        return Ok(new { success = true, isFavorite, message = isFavorite ? "Saved to favorites." : "Removed from favorites." });
    }
}

public class ApiToggleFavoriteRequest
{
    [Required]
    public int CustomerId { get; set; }
    [Required]
    public string Type { get; set; } = "product"; // product or farmer
    [Required]
    public int Id { get; set; }
}
