using Microsoft.AspNetCore.Identity;

namespace MarketLink.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string FullName => $"{FirstName} {LastName}".Trim();

    // Navigation
    public Customer? Customer { get; set; }
    public Farmer? Farmer { get; set; }
    public ICollection<Notification> Notifications { get; set; } = [];
}
