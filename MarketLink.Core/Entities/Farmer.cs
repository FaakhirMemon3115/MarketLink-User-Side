using MarketLink.Core.Enums;

namespace MarketLink.Core.Entities;

public class Farmer
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Bio { get => Description; set => Description = value; }
    public string? StallNumber { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? BannerImageUrl { get; set; }
    public string? ProfileImageUrl { get; set; }
    public FarmerStatus Status { get; set; } = FarmerStatus.Pending;
    public bool IsFeatured { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public string? AdminNotes { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public ICollection<FarmerMarket> FarmerMarkets { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<PickupSlot> PickupSlots { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<Favorite> Favorites { get; set; } = [];
}
