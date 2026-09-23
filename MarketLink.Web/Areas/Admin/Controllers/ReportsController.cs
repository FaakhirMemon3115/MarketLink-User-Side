using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using MarketLink.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text;
using FarmerEntity = MarketLink.Core.Entities.Farmer;
using CustomerEntity = MarketLink.Core.Entities.Customer;

namespace MarketLink.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Platform Reports & Exports";

        var totalOrders    = await _unitOfWork.Repository<Order>().CountAsync();
        var totalFarmers   = await _unitOfWork.Repository<FarmerEntity>().CountAsync();
        var totalCustomers = await _unitOfWork.Repository<CustomerEntity>().CountAsync();
        var totalProducts  = await _unitOfWork.Repository<Product>().CountAsync();

        var completedOrders = await _unitOfWork.Repository<Order>().Query()
            .Where(o => o.Status == OrderStatus.Completed)
            .ToListAsync();

        ViewBag.TotalOrders    = totalOrders;
        ViewBag.TotalFarmers   = totalFarmers;
        ViewBag.TotalCustomers = totalCustomers;
        ViewBag.TotalProducts  = totalProducts;
        ViewBag.TotalRevenue   = completedOrders.Sum(o => o.TotalAmount);

        return View();
    }

    public async Task<IActionResult> Analytics()
    {
        ViewData["Title"] = "Growth & Revenue Analytics";

        var orders    = await _unitOfWork.Repository<Order>().Query().ToListAsync();
        var farmers   = await _unitOfWork.Repository<FarmerEntity>().Query().ToListAsync();
        var customers = await _unitOfWork.Repository<CustomerEntity>().Query().ToListAsync();

        var days = Enumerable.Range(0, 7)
            .Select(i => DateTime.UtcNow.Date.AddDays(-6 + i))
            .ToList();

        ViewBag.DaysLabels   = days.Select(d => d.ToString("ddd (MMM dd)")).ToList();
        ViewBag.OrdersPerDay = days.Select(d => orders.Count(o => o.OrderedAt.Date == d)).ToList();
        ViewBag.RevenuePerDay = days.Select(d =>
            orders.Where(o => o.OrderedAt.Date == d && o.Status != OrderStatus.Cancelled)
                  .Sum(o => o.TotalAmount)).ToList();

        ViewBag.TotalOrders    = orders.Count;
        ViewBag.TotalFarmers   = farmers.Count;
        ViewBag.TotalCustomers = customers.Count;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ExportOrdersCsv()
    {
        var orders = await _unitOfWork.Repository<Order>().Query()
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.Farmer)
            .Include(o => o.PickupSlot).ThenInclude(ps => ps.Market)
            .OrderByDescending(o => o.OrderedAt)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("OrderNumber,Date,Customer,Email,Farmer,Market,ItemsCount,TotalAmount,Status");

        foreach (var o in orders)
        {
            var customerName = (o.Customer?.User?.FullName ?? "Customer").Replace(",", " ");
            var email        = o.Customer?.User?.Email ?? "";
            var farmerName   = (o.Farmer?.FarmName ?? "").Replace(",", " ");
            var marketName   = (o.PickupSlot?.Market?.Name ?? "").Replace(",", " ");

            sb.AppendLine($"{o.OrderNumber},{o.OrderedAt:yyyy-MM-dd HH:mm},{customerName},{email},{farmerName},{marketName},{o.Items.Count},{o.TotalAmount:F2},{o.Status}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"MarketLink_Orders_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ExportFarmersCsv()
    {
        var farmers = await _unitOfWork.Repository<FarmerEntity>().Query()
            .Include(f => f.User)
            .Include(f => f.Products)
            .OrderBy(f => f.FarmName)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Id,FarmName,ContactName,Email,Phone,City,State,ProductsCount,Status,RegisteredDate");

        foreach (var f in farmers)
        {
            var farm    = f.FarmName.Replace(",", " ");
            var contact = (f.User?.FullName ?? "").Replace(",", " ");
            var email   = f.User?.Email ?? "";
            var phone   = f.Phone ?? "";
            var city    = f.City ?? "";
            var state   = f.State ?? "";

            sb.AppendLine($"{f.Id},{farm},{contact},{email},{phone},{city},{state},{f.Products.Count},{f.Status},{f.RegisteredAt:yyyy-MM-dd}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"MarketLink_Farmers_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
