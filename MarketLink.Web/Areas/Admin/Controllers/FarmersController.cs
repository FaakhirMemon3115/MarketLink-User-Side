using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;

namespace MarketLink.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class FarmersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public FarmersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Manage Farmers";
        var farmers = await _unitOfWork.Repository<MarketLink.Core.Entities.Farmer>().GetAllAsync();
        return View(farmers);
    }
}
