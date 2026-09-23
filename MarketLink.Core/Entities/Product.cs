using MarketLink.Core.Enums;

namespace MarketLink.Core.Entities;

public class Product
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PricePerKg { get; set; }
    public int StockQuantityKg { get; set; }
    
    // View Compatibility Properties
    public string? ImageUrl { get; set; }
    public int StockKg { get => StockQuantityKg; set => StockQuantityKg = value; }
    public bool IsActive { get => IsAvailable; set => IsAvailable = value; }
    public string Unit { get; set; } = "KG";
    public bool IsOrganic { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsBestSeller { get; set; }
    public Season Season { get; set; } = Season.AllYear;
    public string? AvailableQuantities { get; set; }  // JSON: [5,10,15,20,25,50,100]
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Farmer Farmer { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<Inventory> Inventories { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<Favorite> Favorites { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
}

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}
