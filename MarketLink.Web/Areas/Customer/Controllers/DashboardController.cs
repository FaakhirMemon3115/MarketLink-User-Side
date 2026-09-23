using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MarketLink.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize(Roles = "Customer")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Customer Dashboard";
        ViewData["Subtitle"] = "Welcome back to your eGreen Basket";
        return View();
    }
}
