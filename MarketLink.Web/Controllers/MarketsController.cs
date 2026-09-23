using Microsoft.AspNetCore.Mvc;
using MarketLink.Core.Interfaces;

namespace MarketLink.Web.Controllers;

public class MarketsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public MarketsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var markets = await _unitOfWork.Repository<MarketLink.Core.Entities.Market>().GetAllAsync();
        return View(markets);
    }

    public async Task<IActionResult> Details(int id)
    {
        var market = await _unitOfWork.Repository<MarketLink.Core.Entities.Market>().GetByIdAsync(id);
        if (market == null) return NotFound();
        return View(market);
    }
}
