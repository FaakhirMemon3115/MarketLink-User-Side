using MarketLink.Core.Entities;
using MarketLink.Core.Interfaces;
using MarketLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _db;

    public CartService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<CartItem>> GetCartAsync(int customerId)
        => await _db.CartItems
            .Include(c => c.Product).ThenInclude(p => p.Images)
            .Include(c => c.Product).ThenInclude(p => p.Farmer).ThenInclude(f => f.User)
            .Where(c => c.CustomerId == customerId)
            .ToListAsync();

    public async Task AddToCartAsync(int customerId, int productId, int quantityKg)
    {
        var existing = await _db.CartItems
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == productId);

        if (existing != null)
        {
            existing.QuantityKg += quantityKg;
            _db.CartItems.Update(existing);
        }
        else
        {
            await _db.CartItems.AddAsync(new CartItem
            {
                CustomerId = customerId,
                ProductId = productId,
                QuantityKg = quantityKg,
                AddedAt = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync();
    }

    public async Task UpdateCartItemAsync(int cartItemId, int quantityKg)
    {
        var item = await _db.CartItems.FindAsync(cartItemId);
        if (item != null)
        {
            item.QuantityKg = quantityKg;
            await _db.SaveChangesAsync();
        }
    }

    public async Task RemoveFromCartAsync(int cartItemId)
    {
        var item = await _db.CartItems.FindAsync(cartItemId);
        if (item != null)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(int customerId)
    {
        var items = _db.CartItems.Where(c => c.CustomerId == customerId);
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    public async Task<decimal> GetCartTotalAsync(int customerId)
    {
        var items = await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.CustomerId == customerId)
            .ToListAsync();
        return items.Sum(i => i.Product.PricePerKg * i.QuantityKg);
    }
}

public class FarmerService : IFarmerService
{
    private readonly ApplicationDbContext _db;

    public FarmerService(ApplicationDbContext db) => _db = db;

    public async Task<(IEnumerable<Farmer> Items, int TotalCount)> GetFarmersAsync(
        string? search, int? marketId, bool? isFeatured, int page, int pageSize)
    {
        var q = _db.Farmers
            .Include(f => f.User)
            .Include(f => f.FarmerMarkets).ThenInclude(fm => fm.Market)
            .Where(f => f.Status == Core.Enums.FarmerStatus.Approved);

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(f => f.FarmName.Contains(search) || f.Description!.Contains(search));
        if (marketId.HasValue)
            q = q.Where(f => f.FarmerMarkets.Any(fm => fm.MarketId == marketId.Value && fm.IsActive));
        if (isFeatured.HasValue)
            q = q.Where(f => f.IsFeatured == isFeatured.Value);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(f => f.IsFeatured)
            .ThenByDescending(f => f.RegisteredAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<Farmer?> GetFarmerByUserIdAsync(string userId)
        => await _db.Farmers
            .Include(f => f.User)
            .Include(f => f.FarmerMarkets).ThenInclude(fm => fm.Market)
            .Include(f => f.Products).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(f => f.UserId == userId);

    public async Task<Farmer> CreateFarmerProfileAsync(Farmer farmer)
    {
        _db.Farmers.Add(farmer);
        await _db.SaveChangesAsync();
        return farmer;
    }

    public async Task<Farmer> UpdateFarmerProfileAsync(Farmer farmer)
    {
        _db.Farmers.Update(farmer);
        await _db.SaveChangesAsync();
        return farmer;
    }

    public async Task<bool> ApproveFarmerAsync(int farmerId)
    {
        var farmer = await _db.Farmers.FindAsync(farmerId);
        if (farmer == null) return false;
        farmer.Status = Core.Enums.FarmerStatus.Approved;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SuspendFarmerAsync(int farmerId, string reason)
    {
        var farmer = await _db.Farmers.FindAsync(farmerId);
        if (farmer == null) return false;
        farmer.Status = Core.Enums.FarmerStatus.Suspended;
        farmer.AdminNotes = reason;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Farmer>> GetFeaturedFarmersAsync(int count = 6)
        => await _db.Farmers
            .Include(f => f.User)
            .Include(f => f.Products).ThenInclude(p => p.Images)
            .Where(f => f.IsFeatured && f.Status == Core.Enums.FarmerStatus.Approved)
            .Take(count).ToListAsync();
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;

    public NotificationService(ApplicationDbContext db) => _db = db;

    public async Task SendAsync(string userId, Core.Enums.NotificationType type, string title, string message, string? link = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Link = link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
    {
        var q = _db.Notifications.Where(n => n.UserId == userId);
        if (unreadOnly) q = q.Where(n => !n.IsRead);
        return await q.OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var n = await _db.Notifications.FindAsync(notificationId);
        if (n != null) { n.IsRead = true; await _db.SaveChangesAsync(); }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task<int> GetUnreadCountAsync(string userId)
        => await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
}
