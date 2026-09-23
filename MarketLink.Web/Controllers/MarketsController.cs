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
        var markets = await _unitOfWork.Markets.GetAllAsync();
        return View(markets);
    }

    public async Task<IActionResult> Details(int id)
    {
        var market = await _unitOfWork.Markets.GetByIdAsync(id);
        if (market == null) return NotFound();
        return View(market);
    }
}
