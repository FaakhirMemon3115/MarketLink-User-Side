using MarketLink.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Farmer> Farmers => Set<Farmer>();
    public DbSet<FarmerMarket> FarmerMarkets => Set<FarmerMarket>();
    public DbSet<Market> Markets => Set<Market>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<PickupSlot> PickupSlots => Set<PickupSlot>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Newsletter> Newsletters => Set<Newsletter>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── ApplicationUser ────────────────────────────────────────────
        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        });

        // ── Customer ──────────────────────────────────────────────────
        builder.Entity<Customer>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User)
             .WithOne(x => x.Customer)
             .HasForeignKey<Customer>(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CustomerAddress>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Customer)
             .WithMany(x => x.Addresses)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Label).HasMaxLength(50);
            e.Property(x => x.AddressLine1).HasMaxLength(200);
            e.Property(x => x.City).HasMaxLength(100);
        });

        // ── Farmer ────────────────────────────────────────────────────
        builder.Entity<Farmer>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User)
             .WithOne(x => x.Farmer)
             .HasForeignKey<Farmer>(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.FarmName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Status).HasConversion<string>();
        });

        // ── FarmerMarket ──────────────────────────────────────────────
        builder.Entity<FarmerMarket>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.FarmerId, x.MarketId }).IsUnique();
            e.HasOne(x => x.Farmer)
             .WithMany(x => x.FarmerMarkets)
             .HasForeignKey(x => x.FarmerId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Market)
             .WithMany(x => x.FarmerMarkets)
             .HasForeignKey(x => x.MarketId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Market ────────────────────────────────────────────────────
        builder.Entity<Market>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.City).HasMaxLength(100).IsRequired();
        });

        // ── Category ──────────────────────────────────────────────────
        builder.Entity<Category>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(120).IsRequired();
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasOne(x => x.Parent)
             .WithMany(x => x.Children)
             .HasForeignKey(x => x.ParentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Product ───────────────────────────────────────────────────
        builder.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(220).IsRequired();
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.PricePerKg).HasPrecision(10, 2);
            e.Property(x => x.Season).HasConversion<string>();
            e.HasOne(x => x.Farmer)
             .WithMany(x => x.Products)
             .HasForeignKey(x => x.FarmerId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category)
             .WithMany(x => x.Products)
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductImage>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Product)
             .WithMany(x => x.Images)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Inventory ─────────────────────────────────────────────────
        builder.Entity<Inventory>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ProductId, x.FarmerId, x.WeekNumber, x.Year }).IsUnique();
            e.Property(x => x.PricePerKg).HasPrecision(10, 2);
            e.HasOne(x => x.Product)
             .WithMany(x => x.Inventories)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Farmer)
             .WithMany()
             .HasForeignKey(x => x.FarmerId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── PickupSlot ────────────────────────────────────────────────
        builder.Entity<PickupSlot>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Farmer)
             .WithMany(x => x.PickupSlots)
             .HasForeignKey(x => x.FarmerId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Market)
             .WithMany(x => x.PickupSlots)
             .HasForeignKey(x => x.MarketId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Order ─────────────────────────────────────────────────────
        builder.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderNumber).IsUnique();
            e.Property(x => x.OrderNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.SubTotal).HasPrecision(10, 2);
            e.Property(x => x.TotalAmount).HasPrecision(10, 2);
            e.HasOne(x => x.Customer)
             .WithMany(x => x.Orders)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Farmer)
             .WithMany(x => x.Orders)
             .HasForeignKey(x => x.FarmerId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PickupSlot)
             .WithMany(x => x.Orders)
             .HasForeignKey(x => x.PickupSlotId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PricePerKgSnapshot).HasPrecision(10, 2);
            e.Property(x => x.TotalPrice).HasPrecision(10, 2);
            e.HasOne(x => x.Order)
             .WithMany(x => x.Items)
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product)
             .WithMany(x => x.OrderItems)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── CartItem ──────────────────────────────────────────────────
        builder.Entity<CartItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.CustomerId, x.ProductId }).IsUnique();
            e.HasOne(x => x.Customer)
             .WithMany(x => x.CartItems)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product)
             .WithMany(x => x.CartItems)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Favorite ──────────────────────────────────────────────────
        builder.Entity<Favorite>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Customer)
             .WithMany(x => x.Favorites)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product)
             .WithMany(x => x.Favorites)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Farmer)
             .WithMany(x => x.Favorites)
             .HasForeignKey(x => x.FarmerId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── Review ────────────────────────────────────────────────────
        builder.Entity<Review>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Customer)
             .WithMany(x => x.Reviews)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Product)
             .WithMany(x => x.Reviews)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Farmer)
             .WithMany(x => x.Reviews)
             .HasForeignKey(x => x.FarmerId)
             .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Order)
             .WithMany()
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── Notification ──────────────────────────────────────────────
        builder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasConversion<string>();
            e.HasOne(x => x.User)
             .WithMany(x => x.Notifications)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── SiteSetting ───────────────────────────────────────────────
        builder.Entity<SiteSetting>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Key).IsUnique();
            e.Property(x => x.Key).HasMaxLength(100).IsRequired();
        });

        // ── Report ────────────────────────────────────────────────────
        builder.Entity<Report>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasConversion<string>();
        });
    }
}
