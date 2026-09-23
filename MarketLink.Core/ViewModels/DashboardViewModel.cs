using MarketLink.Core.Entities;

namespace MarketLink.Core.ViewModels;

public class DashboardViewModel
{
    public string CustomerName { get; set; } = string.Empty;
    public string? DefaultCity { get; set; }
    public int TotalOrdersCount { get; set; }
    public int TotalFavoritesCount { get; set; }
    public int TotalAddressesCount { get; set; }
    public decimal TotalSpent { get; set; }

    public IEnumerable<Product> Products { get; set; } = [];
    public IEnumerable<Category> Categories { get; set; } = [];
    public IEnumerable<Favorite> Favorites { get; set; } = [];
    public IEnumerable<Order> RecentOrders { get; set; } = [];
    public HashSet<int> FavoriteProductIds { get; set; } = [];
}
