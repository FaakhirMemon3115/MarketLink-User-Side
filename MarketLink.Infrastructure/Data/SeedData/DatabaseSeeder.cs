using MarketLink.Core.Entities;
using MarketLink.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace MarketLink.Infrastructure.Data.SeedData;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Roles
        string[] roles = ["Admin", "Farmer", "Customer"];
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        await SeedSettingsAsync(db);
        await SeedCategoriesAsync(db);
        await SeedMarketsAsync(db);
        await SeedUsersAsync(db, userManager);
    }

    private static async Task SeedSettingsAsync(ApplicationDbContext db)
    {
        if (db.SiteSettings.Any()) return;
        db.SiteSettings.AddRange(
            new SiteSetting { Key = "SiteName", Value = "MarketLink – eGreen Basket", Group = "General" },
            new SiteSetting { Key = "SiteTagline", Value = "Farm Fresh, Delivered Local", Group = "General" },
            new SiteSetting { Key = "ContactEmail", Value = "info@marketlink.com", Group = "Contact" },
            new SiteSetting { Key = "ContactPhone", Value = "+1 (555) 123-4567", Group = "Contact" },
            new SiteSetting { Key = "FacebookUrl", Value = "https://facebook.com/marketlink", Group = "Social" },
            new SiteSetting { Key = "InstagramUrl", Value = "https://instagram.com/marketlink", Group = "Social" },
            new SiteSetting { Key = "MaintenanceMode", Value = "false", Group = "System" }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext db)
    {
        if (db.Categories.Any()) return;

        var cats = new[]
        {
            new Category { Name = "Vegetables", Slug = "vegetables", IconClass = "fas fa-carrot", SortOrder = 1 },
            new Category { Name = "Fruits", Slug = "fruits", IconClass = "fas fa-apple-alt", SortOrder = 2 },
            new Category { Name = "Dairy", Slug = "dairy", IconClass = "fas fa-cheese", SortOrder = 3 },
            new Category { Name = "Herbs", Slug = "herbs", IconClass = "fas fa-leaf", SortOrder = 4 },
            new Category { Name = "Grains", Slug = "grains", IconClass = "fas fa-seedling", SortOrder = 5 },
        };
        db.Categories.AddRange(cats);
        await db.SaveChangesAsync();

        // Sub-categories
        var vegs = db.Categories.First(c => c.Slug == "vegetables");
        var fruits = db.Categories.First(c => c.Slug == "fruits");
        var dairy = db.Categories.First(c => c.Slug == "dairy");
        var herbs = db.Categories.First(c => c.Slug == "herbs");
        var grains = db.Categories.First(c => c.Slug == "grains");

        db.Categories.AddRange([
            new Category { Name = "Potato", Slug = "potato", ParentId = vegs.Id, SortOrder = 1 },
            new Category { Name = "Onion", Slug = "onion", ParentId = vegs.Id, SortOrder = 2 },
            new Category { Name = "Tomato", Slug = "tomato", ParentId = vegs.Id, SortOrder = 3 },
            new Category { Name = "Garlic", Slug = "garlic", ParentId = vegs.Id, SortOrder = 4 },
            new Category { Name = "Carrot", Slug = "carrot", ParentId = vegs.Id, SortOrder = 5 },
            new Category { Name = "Spinach", Slug = "spinach", ParentId = vegs.Id, SortOrder = 6 },
            new Category { Name = "Cabbage", Slug = "cabbage", ParentId = vegs.Id, SortOrder = 7 },
            new Category { Name = "Capsicum", Slug = "capsicum", ParentId = vegs.Id, SortOrder = 8 },
            new Category { Name = "Cucumber", Slug = "cucumber", ParentId = vegs.Id, SortOrder = 9 },
            new Category { Name = "Ginger", Slug = "ginger", ParentId = vegs.Id, SortOrder = 10 },

            new Category { Name = "Mango", Slug = "mango", ParentId = fruits.Id, SortOrder = 1 },
            new Category { Name = "Apple", Slug = "apple", ParentId = fruits.Id, SortOrder = 2 },
            new Category { Name = "Banana", Slug = "banana", ParentId = fruits.Id, SortOrder = 3 },
            new Category { Name = "Orange", Slug = "orange", ParentId = fruits.Id, SortOrder = 4 },
            new Category { Name = "Pomegranate", Slug = "pomegranate", ParentId = fruits.Id, SortOrder = 5 },
            new Category { Name = "Grapes", Slug = "grapes", ParentId = fruits.Id, SortOrder = 6 },
            new Category { Name = "Guava", Slug = "guava", ParentId = fruits.Id, SortOrder = 7 },
            new Category { Name = "Watermelon", Slug = "watermelon", ParentId = fruits.Id, SortOrder = 8 },
            new Category { Name = "Melon", Slug = "melon", ParentId = fruits.Id, SortOrder = 9 },
            new Category { Name = "Peach", Slug = "peach", ParentId = fruits.Id, SortOrder = 10 },

            new Category { Name = "Milk", Slug = "milk", ParentId = dairy.Id, SortOrder = 1 },
            new Category { Name = "Yogurt", Slug = "yogurt", ParentId = dairy.Id, SortOrder = 2 },
            new Category { Name = "Butter", Slug = "butter", ParentId = dairy.Id, SortOrder = 3 },
            new Category { Name = "Cheese", Slug = "cheese", ParentId = dairy.Id, SortOrder = 4 },

            new Category { Name = "Mint", Slug = "mint", ParentId = herbs.Id, SortOrder = 1 },
            new Category { Name = "Coriander", Slug = "coriander", ParentId = herbs.Id, SortOrder = 2 },
            new Category { Name = "Basil", Slug = "basil", ParentId = herbs.Id, SortOrder = 3 },

            new Category { Name = "Wheat", Slug = "wheat", ParentId = grains.Id, SortOrder = 1 },
            new Category { Name = "Rice", Slug = "rice", ParentId = grains.Id, SortOrder = 2 },
            new Category { Name = "Corn", Slug = "corn", ParentId = grains.Id, SortOrder = 3 },
        ]);
        await db.SaveChangesAsync();
    }

    private static async Task SeedMarketsAsync(ApplicationDbContext db)
    {
        if (db.Markets.Any()) return;

        db.Markets.AddRange([
            new Market
            {
                Name = "Green Valley Farmers Market",
                Description = "The oldest and most beloved farmers market in the region. Come experience fresh local produce every Saturday!",
                Address = "123 Main Street",
                City = "Springfield",
                State = "IL",
                Latitude = 39.7817,
                Longitude = -89.6501,
                OperatingHours = "7:00 AM - 2:00 PM",
                OpenDays = "Saturday,Sunday",
                Phone = "+1 (555) 234-5678",
                Email = "greenvalley@markets.com",
                IsActive = true,
                IsFeatured = true
            },
            new Market
            {
                Name = "Riverside Organic Market",
                Description = "Certified organic produce direct from local farms. Fresh herbs, vegetables and seasonal fruits.",
                Address = "456 River Road",
                City = "Riverside",
                State = "CA",
                Latitude = 33.9806,
                Longitude = -117.3755,
                OperatingHours = "8:00 AM - 3:00 PM",
                OpenDays = "Wednesday,Saturday",
                IsActive = true,
                IsFeatured = true
            },
            new Market
            {
                Name = "Harvest Moon Market",
                Description = "Monthly harvest festival market featuring seasonal produce, artisanal goods, and live music.",
                Address = "789 Harvest Lane",
                City = "Portland",
                State = "OR",
                Latitude = 45.5152,
                Longitude = -122.6784,
                OperatingHours = "9:00 AM - 4:00 PM",
                OpenDays = "Sunday",
                IsActive = true,
                IsFeatured = false
            },
            new Market
            {
                Name = "Downtown Fresh Market",
                Description = "Convenient downtown location for busy shoppers. Premium quality local produce.",
                Address = "321 Downtown Ave",
                City = "Chicago",
                State = "IL",
                Latitude = 41.8781,
                Longitude = -87.6298,
                OperatingHours = "10:00 AM - 6:00 PM",
                OpenDays = "Tuesday,Thursday,Saturday",
                IsActive = true,
                IsFeatured = false
            },
        ]);
        await db.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        // Admin
        if (await userManager.FindByEmailAsync("admin@marketlink.com") == null)
        {
            var admin = new ApplicationUser
            {
                FirstName = "System",
                LastName = "Administrator",
                UserName = "admin@marketlink.com",
                Email = "admin@marketlink.com",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(admin, "Admin@123456");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Farmers
        var farmerData = new[]
        {
            ("john@greenfarms.com", "John", "Harrison", "Harrison Organic Farms", "Specializing in heirloom vegetables and seasonal produce. Family farm since 1985.", "A1"),
            ("sarah@sunnyside.com", "Sarah", "Miller", "Sunnyside Dairy Farm", "Award-winning dairy farm producing artisanal cheese, yogurt, and fresh milk.", "B3"),
            ("miguel@miguelsfarm.com", "Miguel", "Rodriguez", "Miguel's Heritage Farm", "Traditional farming methods combined with modern organic practices.", "C7"),
            ("emma@herbgarden.com", "Emma", "Chen", "Emma's Herb Garden", "Specialty herb farm growing over 30 varieties of culinary and medicinal herbs.", "D2"),
            ("david@riverbanks.com", "David", "Thompson", "Riverbanks Fresh Produce", "Prime location riverside farm. Fresh fruits and vegetables picked daily.", "E5"),
        };

        var markets = await System.Threading.Tasks.Task.FromResult(db.Markets.ToList());
        var categories = await System.Threading.Tasks.Task.FromResult(db.Categories.ToList());
        var vegCategory = categories.First(c => c.Slug == "vegetables");
        var fruitCategory = categories.First(c => c.Slug == "fruits");
        var dairyCategory = categories.First(c => c.Slug == "dairy");
        var herbCategory = categories.First(c => c.Slug == "herbs");

        int farmerIndex = 0;
        foreach (var (email, first, last, farmName, desc, stall) in farmerData)
        {
            if (await userManager.FindByEmailAsync(email) != null) { farmerIndex++; continue; }

            var user = new ApplicationUser
            {
                FirstName = first,
                LastName = last,
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(user, "Farmer@123456");
            await userManager.AddToRoleAsync(user, "Farmer");

            var farmer = new Farmer
            {
                UserId = user.Id,
                FarmName = farmName,
                Description = desc,
                StallNumber = stall,
                Status = FarmerStatus.Approved,
                IsFeatured = farmerIndex < 3,
                RegisteredAt = DateTime.UtcNow.AddDays(-90 + farmerIndex * 10)
            };
            db.Farmers.Add(farmer);
            await db.SaveChangesAsync();

            // Assign to market
            if (markets.Count > farmerIndex)
            {
                db.FarmerMarkets.Add(new FarmerMarket
                {
                    FarmerId = farmer.Id,
                    MarketId = markets[farmerIndex % markets.Count].Id,
                    StallNumber = stall,
                    IsActive = true
                });
                await db.SaveChangesAsync();
            }

            // Add sample products
            var productCategory = farmerIndex switch
            {
                1 => dairyCategory,
                3 => herbCategory,
                _ => vegCategory
            };

            var product = new Product
            {
                FarmerId = farmer.Id,
                CategoryId = productCategory.Id,
                Name = farmerIndex switch
                {
                    0 => "Heirloom Tomatoes",
                    1 => "Fresh Whole Milk",
                    2 => "Organic Potatoes",
                    3 => "Fresh Basil Bundle",
                    4 => "Mixed Salad Greens",
                    _ => "Fresh Vegetables"
                },
                Slug = $"{farmName.ToLower().Replace(" ", "-")}-product-{farmer.Id}",
                Description = $"Premium quality produce from {farmName}. Grown with care using sustainable farming practices.",
                PricePerKg = (farmerIndex + 1) * 2.5m,
                StockQuantityKg = 500,
                IsOrganic = farmerIndex % 2 == 0,
                IsAvailable = true,
                IsFeatured = farmerIndex < 2,
                IsBestSeller = farmerIndex < 2,
                Season = Season.AllYear,
                AvailableQuantities = "[5,10,15,20,25,50]",
                CreatedAt = DateTime.UtcNow
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            db.ProductImages.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = $"/images/products/sample-{farmerIndex + 1}.jpg",
                IsPrimary = true,
                SortOrder = 0
            });
            await db.SaveChangesAsync();

            // Pickup slot
            db.PickupSlots.Add(new PickupSlot
            {
                FarmerId = farmer.Id,
                MarketId = markets.Count > farmerIndex ? markets[farmerIndex % markets.Count].Id : null,
                DayOfWeek = (DayOfWeek)(farmerIndex % 7),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(12, 0),
                MaxOrders = 20,
                IsActive = true
            });
            await db.SaveChangesAsync();

            farmerIndex++;
        }

        // Customers
        var customerData = new[]
        {
            ("alice@example.com", "Alice", "Johnson"),
            ("bob@example.com", "Bob", "Williams"),
            ("carol@example.com", "Carol", "Davis"),
        };

        foreach (var (email, first, last) in customerData)
        {
            if (await userManager.FindByEmailAsync(email) != null) continue;

            var user = new ApplicationUser
            {
                FirstName = first,
                LastName = last,
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(user, "Customer@123456");
            await userManager.AddToRoleAsync(user, "Customer");

            var customer = new Customer
            {
                UserId = user.Id,
                DefaultCity = "Springfield"
            };
            db.Customers.Add(customer);
            await db.SaveChangesAsync();

            db.CustomerAddresses.Add(new CustomerAddress
            {
                CustomerId = customer.Id,
                Label = "Home",
                AddressLine1 = "123 Sample Street",
                City = "Springfield",
                State = "IL",
                PostalCode = "62701",
                IsDefault = true
            });
            await db.SaveChangesAsync();
        }
    }
}
