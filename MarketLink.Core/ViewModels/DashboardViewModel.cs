namespace MarketLink.Core.ViewModels;

public class DashboardViewModel
{
    public IEnumerable<MarketLink.Core.Entities.Product> Products { get; set; } = [];
    public IEnumerable<MarketLink.Core.Entities.Favorite> Favorites { get; set; } = [];
    public IEnumerable<MarketLink.Core.Entities.Order> RecentOrders { get; set; } = [];
}
