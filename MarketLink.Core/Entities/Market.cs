namespace MarketLink.Core.Entities;

public class Market
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ImageUrl { get; set; }
    public string? OperatingHours { get; set; }
    public string? OpenDays { get; set; }  // JSON or comma-separated
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<FarmerMarket> FarmerMarkets { get; set; } = [];
    public ICollection<PickupSlot> PickupSlots { get; set; } = [];
}

public class FarmerMarket
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public int MarketId { get; set; }
    public string? StallNumber { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Farmer Farmer { get; set; } = null!;
    public Market Market { get; set; } = null!;
}
