using Microsoft.AspNetCore.Mvc;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using MarketLink.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MarketLink.Web.Controllers.Api;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public class OrdersApiController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderService _orderService;

    public OrdersApiController(IUnitOfWork unitOfWork, IOrderService orderService)
    {
        _unitOfWork = unitOfWork;
        _orderService = orderService;
    }

    [HttpGet("customer/{customerId:int}")]
    public async Task<IActionResult> GetCustomerOrders(int customerId)
    {
        var orders = await _unitOfWork.Repository<Order>().Query()
            .Include(o => o.Farmer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderedAt)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.Status,
                o.TotalAmount,
                o.PaymentMethod,
                o.OrderedAt,
                o.CompletedAt,
                Farmer = new { o.Farmer.Id, o.Farmer.FarmName },
                Items = o.Items.Select(i => new
                {
                    i.ProductId,
                    i.ProductNameSnapshot,
                    i.QuantityKg,
                    i.PricePerKgSnapshot,
                    i.TotalPrice
                })
            })
            .ToListAsync();

        return Ok(new { success = true, count = orders.Count, orders });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var o = await _unitOfWork.Repository<Order>().Query()
            .Include(o => o.Farmer).ThenInclude(f => f.User)
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (o == null) return NotFound(new { success = false, message = "Order not found." });

        return Ok(new
        {
            success = true,
            order = new
            {
                o.Id,
                o.OrderNumber,
                o.Status,
                o.TotalAmount,
                o.SubTotal,
                o.Notes,
                o.PaymentMethod,
                o.OrderedAt,
                o.PickupTime,
                o.CompletedAt,
                Customer = new { o.Customer.Id, CustomerName = $"{o.Customer.User.FirstName} {o.Customer.User.LastName}".Trim(), o.Customer.User.PhoneNumber },
                Farmer = new { o.Farmer.Id, o.Farmer.FarmName, o.Farmer.StallNumber },
                Items = o.Items.Select(i => new
                {
                    i.ProductId,
                    i.ProductNameSnapshot,
                    i.QuantityKg,
                    i.PricePerKgSnapshot,
                    i.TotalPrice
                })
            }
        });
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] ApiCreateOrderRequest request)
    {
        if (!ModelState.IsValid || !request.Items.Any())
            return BadRequest(new { success = false, message = "Order items are required." });

        try
        {
            var cartItems = request.Items.Select(i => new CartItem
            {
                CustomerId = request.CustomerId,
                ProductId = i.ProductId,
                QuantityKg = i.QuantityKg
            });

            var order = await _orderService.PlaceOrderAsync(request.CustomerId, cartItems, request.PickupSlotId ?? 1, request.Notes ?? "Cash on Pickup");
            return StatusCode(201, new
            {
                success = true,
                message = "Pre-order placed successfully.",
                orderId = order.Id,
                orderNumber = order.OrderNumber,
                totalAmount = order.TotalAmount
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public class ApiCreateOrderRequest
{
    [Required]
    public int CustomerId { get; set; }
    public int? PickupSlotId { get; set; }
    public string? Notes { get; set; }
    [Required]
    public List<ApiOrderItemRequest> Items { get; set; } = [];
}

public class ApiOrderItemRequest
{
    public int ProductId { get; set; }
    public int QuantityKg { get; set; }
}
