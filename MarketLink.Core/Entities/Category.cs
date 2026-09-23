namespace MarketLink.Core.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; }   // Font Awesome class
    public string? ImageUrl { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
