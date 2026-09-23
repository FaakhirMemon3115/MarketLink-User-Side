using Microsoft.AspNetCore.Mvc;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using MarketLink.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Controllers.Api;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsApiController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsApiController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? q,
        [FromQuery] string? category,
        [FromQuery] bool? isOrganic,
        [FromQuery] bool? inSeason,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sortBy = "newest",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var query = _unitOfWork.Repository<Product>().Query()
            .Include(p => p.Category)
            .Include(p => p.Farmer).ThenInclude(f => f.User)
            .Include(p => p.Images)
            .Where(p => p.IsAvailable && p.Farmer.Status == FarmerStatus.Approved);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim();
            query = query.Where(p => p.Name.Contains(search) || 
                                     (p.Description != null && p.Description.Contains(search)) ||
                                     (p.Tags != null && p.Tags.Contains(search)) ||
                                     p.Farmer.FarmName.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category.Slug == category || (p.Category.Parent != null && p.Category.Parent.Slug == category));
        }

        if (isOrganic.HasValue)
        {
            query = query.Where(p => p.IsOrganic == isOrganic.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.PricePerKg >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.PricePerKg <= maxPrice.Value);
        }

        query = sortBy switch
        {
            "price_asc" => query.OrderBy(p => p.PricePerKg),
            "price_desc" => query.OrderByDescending(p => p.PricePerKg),
            "name" => query.OrderBy(p => p.Name),
            "bestseller" => query.OrderByDescending(p => p.IsBestSeller).ThenByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.Description,
                p.PricePerKg,
                p.StockQuantityKg,
                p.Unit,
                p.IsOrganic,
                p.IsFeatured,
                p.IsBestSeller,
                p.Season,
                ImageUrl = p.ImageUrl ?? (p.Images.FirstOrDefault() != null ? p.Images.FirstOrDefault()!.ImageUrl : null),
                Category = new { p.Category.Id, p.Category.Name, p.Category.Slug },
                Farmer = new { p.Farmer.Id, p.Farmer.FarmName, FarmerName = $"{p.Farmer.User.FirstName} {p.Farmer.User.LastName}".Trim() }
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize),
            products = items
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _unitOfWork.Repository<Product>().Query()
            .Include(p => p.Category)
            .Include(p => p.Farmer).ThenInclude(f => f.User)
            .Include(p => p.Images)
            .Include(p => p.Reviews).ThenInclude(r => r.Customer).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return NotFound(new { success = false, message = "Product not found." });

        return Ok(new
        {
            success = true,
            product = new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.Description,
                p.PricePerKg,
                p.StockQuantityKg,
                p.Unit,
                p.IsOrganic,
                p.IsFeatured,
                p.IsBestSeller,
                p.Season,
                ImageUrl = p.ImageUrl ?? (p.Images.FirstOrDefault() != null ? p.Images.FirstOrDefault()!.ImageUrl : null),
                Images = p.Images.Select(i => i.ImageUrl),
                Category = new { p.Category.Id, p.Category.Name, p.Category.Slug },
                Farmer = new { p.Farmer.Id, p.Farmer.FarmName, p.Farmer.StallNumber, p.Farmer.Bio },
                Reviews = p.Reviews.Where(r => r.IsApproved).Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Title,
                    r.Comment,
                    r.CreatedAt,
                    CustomerName = $"{r.Customer.User.FirstName} {r.Customer.User.LastName}".Trim()
                })
            }
        });
    }
}
