using Microsoft.AspNetCore.Mvc;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Controllers.Api;

[ApiController]
[Route("api/markets")]
[Produces("application/json")]
public class MarketsApiController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public MarketsApiController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetMarkets([FromQuery] string? city, [FromQuery] string? day)
    {
        var query = _unitOfWork.Repository<Market>().Query()
            .Include(m => m.FarmerMarkets).ThenInclude(fm => fm.Farmer)
            .Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(m => m.City.Contains(city));

        if (!string.IsNullOrWhiteSpace(day))
            query = query.Where(m => m.OpenDays.Contains(day));

        var markets = await query
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Description,
                m.Address,
                m.City,
                m.State,
                m.PostalCode,
                m.Latitude,
                m.Longitude,
                m.OperatingHours,
                m.OpenDays,
                m.ImageUrl,
                m.IsFeatured,
                ActiveFarmersCount = m.FarmerMarkets.Count
            })
            .ToListAsync();

        return Ok(new { success = true, count = markets.Count, markets });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMarketById(int id)
    {
        var m = await _unitOfWork.Repository<Market>().Query()
            .Include(m => m.FarmerMarkets).ThenInclude(fm => fm.Farmer).ThenInclude(f => f.User)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (m == null) return NotFound(new { success = false, message = "Market not found." });

        return Ok(new
        {
            success = true,
            market = new
            {
                m.Id,
                m.Name,
                m.Description,
                m.Address,
                m.City,
                m.State,
                m.PostalCode,
                m.Latitude,
                m.Longitude,
                m.OperatingHours,
                m.OpenDays,
                m.ImageUrl,
                Farmers = m.FarmerMarkets.Select(fm => new
                {
                    fm.Farmer.Id,
                    fm.Farmer.FarmName,
                    fm.StallLocation,
                    FarmerName = $"{fm.Farmer.User.FirstName} {fm.Farmer.User.LastName}".Trim()
                })
            }
        });
    }
}
