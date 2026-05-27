using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReMarket.Models;
using ReMarket.Utility;

namespace ReMarket.DataAccess.Data
{
    public static class SeedData
    {
        private static readonly string[] DemoItemSlugs = ["iphone-12", "office-chair", "leather-jacket", "macbook-pro-14", "modern-leather-sofa", "samsung-galaxy-s23-ultra", "wooden-dining-table", "pragmatic-programmer-book", "harry-potter-book-set"];

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed Roles
            foreach (var role in new[] { SD.Role_Admin, SD.Role_Customer })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Categories
            await SeedCategoriesAsync(db);

            // Seed Admin User
            var admin = await EnsureAdminUserAsync(userManager);

            // Seed Customer1 and Customer2
            var customer1 = await EnsureCustomerUserAsync(userManager, "customer1@remarket.com", "Customer1@123");
            var customer2 = await EnsureCustomerUserAsync(userManager, "customer2@remarket.com", "Customer2@123");

            // Seed Demo Items distributed among Admin, Customer1, and Customer2
            await SeedDemoItemsAsync(db, admin.Id, customer1.Id, customer2.Id);
        }

        private static async Task<ApplicationUser> EnsureAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            const string email = "admin@remarket.com";
            var admin = await userManager.FindByEmailAsync(email);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    NormalizedUserName = "ADMIN@REMARKET.COM",
                    NormalizedEmail = "ADMIN@REMARKET.COM",
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User",
                    City = "Auckland",
                    Country = "New Zealand"
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, SD.Role_Admin);
            }

            return admin;
        }

        private static async Task<ApplicationUser> EnsureCustomerUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                var firstName = email.Contains("customer1") ? "Customer1" : "Customer";
                var lastName = email.Contains("customer1") ? "Customer2" : "Customer";

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    NormalizedUserName = email.ToUpperInvariant(),
                    NormalizedEmail = email.ToUpperInvariant(),
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    City = email.Contains("customer1") ? "Wellington" : "Christchurch",
                    Country = "New Zealand"
                };
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, SD.Role_Customer);
            }

            return user;
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext db)
        {
            var topLevel = new (string Name, string Description, string Slug)[]
            {
                ("Electronics", "Gadgets and devices", "electronics"),
                ("Furniture", "Home and office furniture", "furniture"),
                ("Clothing", "Apparel and accessories", "clothing"),
                ("Books", "Fiction and non-fiction books", "books"),
                ("Toys & Games", "Toys and board games", "toys-games")
            };

            foreach (var (name, description, slug) in topLevel)
            {
                if (await db.Categories.AnyAsync(c => c.Slug == slug))
                    continue;

                db.Categories.Add(new Category { Name = name, Description = description, Slug = slug });
            }

            await db.SaveChangesAsync();

            var parentIds = await db.Categories
                .Where(c => c.Slug != null)
                .ToDictionaryAsync(c => c.Slug!, c => c.Id);

            var subCategories = new (string Name, string Description, string Slug, string ParentSlug)[]
            {
                ("Mobile Phones", "Smartphones and accessories", "mobile-phones", "electronics"),
                ("Laptops", "Notebooks and accessories", "laptops", "electronics"),
                ("Audio & Headphones", "Earphones, speakers, and soundbars", "audio-headphones", "electronics"),
                ("Sofas", "Comfortable seating", "sofas", "furniture"),
                ("Tables", "Dining and office tables", "tables", "furniture"),
                ("Chairs", "Office, gaming, and lounge chairs", "chairs", "furniture"),
                ("Men's Clothing", "Shirts, pants, jackets for men", "mens-clothing", "clothing"),
                ("Women's Clothing", "Dresses, tops, skirts for women", "womens-clothing", "clothing"),
                ("Kids' Clothing", "Clothing for children", "kids-clothing", "clothing"),
                ("Fiction", "Novels and stories", "fiction", "books"),
                ("Biography", "Biographies and memoirs of notable figures", "biography", "books"),
                ("Educational", "Textbooks and learning materials", "educational", "books"),
                ("Board Games", "Strategy and family board games", "board-games", "toys-games"),
                ("Outdoor Toys", "Bikes, scooters, and playground equipment", "outdoor-toys", "toys-games"),
                ("Puzzles", "Jigsaw and brain teasers", "puzzles", "toys-games")
            };

            foreach (var (name, description, slug, parentSlug) in subCategories)
            {
                if (await db.Categories.AnyAsync(c => c.Slug == slug))
                    continue;

                if (!parentIds.TryGetValue(parentSlug, out var parentId))
                    continue;

                db.Categories.Add(new Category
                {
                    Name = name,
                    Description = description,
                    Slug = slug,
                    ParentCategoryId = parentId
                });
            }

            await db.SaveChangesAsync();
        }

        private static async Task<Dictionary<string, int>> GetCategoryIdsBySlugAsync(ApplicationDbContext db)
        {
            return await db.Categories
                .Where(c => c.Slug != null)
                .ToDictionaryAsync(c => c.Slug!, c => c.Id);
        }

        private static int? ResolveCategoryId(Dictionary<string, int> categoryIds, string slug)
        {
            return categoryIds.TryGetValue(slug, out var id) ? id : null;
        }

        private static async Task SeedDemoItemsAsync(ApplicationDbContext db, string adminId, string customer1Id, string customer2Id)
        {
            var categoryIds = await GetCategoryIdsBySlugAsync(db);

            var existingSlugs = await db.Items
                .Where(i => i.Slug != null && DemoItemSlugs.Contains(i.Slug))
                .Select(i => i.Slug!)
                .ToHashSetAsync();

            var demoItems = new List<Item>();

            void TryAdd(Item item, string categorySlug)
            {
                var categoryId = ResolveCategoryId(categoryIds, categorySlug);
                if (categoryId == null || existingSlugs.Contains(item.Slug!))
                    return;

                item.CategoryId = categoryId.Value;
                demoItems.Add(item);
            }

            // Items from Admin
            TryAdd(new Item
            {
                Name = "MacBook Pro 14\"",
                Slug = "macbook-pro-14",
                Description = "Apple MacBook Pro with M2 chip, 16GB RAM, 512GB SSD. Like new condition, comes with original charger.",
                Price = 1899.99m,
                DeliveryOption = DeliveryOption.Shipping,
                Condition = Condition.LikeNew,
                Status = ItemStatus.Available,
                SellerId = adminId,
                Location = "Auckland",
                Quantity = 1
            }, "laptops");

            TryAdd(new Item
            {
                Name = "Modern Leather Sofa",
                Slug = "modern-leather-sofa",
                Description = "Genuine leather 3-seater sofa in excellent condition. Dark brown color, comfortable and durable.",
                Price = 799.99m,
                DeliveryOption = DeliveryOption.ShippingAndPickup,
                Condition = Condition.Good,
                Status = ItemStatus.Available,
                SellerId = adminId,
                Location = "Auckland",
                Quantity = 1
            }, "sofas");

            // Items from Customer1
            TryAdd(new Item
            {
                Name = "iPhone 12",
                Slug = "iphone-12",
                Description = "A used iPhone 12 in good condition.",
                Price = 499.99m,
                DeliveryOption = DeliveryOption.Pickup,
                Condition = Condition.Good,
                Status = ItemStatus.Pending,
                SellerId = customer1Id,
                Location = "Auckland",
                Quantity = 1
            }, "mobile-phones");

            TryAdd(new Item
            {
                Name = "Office Chair",
                Slug = "office-chair",
                Description = "Ergonomic office chair with adjustable height.",
                Price = 149.99m,
                DeliveryOption = DeliveryOption.Shipping,
                Condition = Condition.New,
                Status = ItemStatus.Available,
                SellerId = customer1Id,
                Location = "Wellington",
                Quantity = 2
            }, "chairs");

            TryAdd(new Item
            {
                Name = "Samsung Galaxy S23 Ultra",
                Slug = "samsung-galaxy-s23-ultra",
                Description = "Latest Samsung flagship with 256GB storage. Includes original box and accessories.",
                Price = 1099.99m,
                DeliveryOption = DeliveryOption.Pickup,
                Condition = Condition.New,
                Status = ItemStatus.Available,
                SellerId = customer1Id,
                Location = "Auckland",
                Quantity = 1
            }, "mobile-phones");

            TryAdd(new Item
            {
                Name = "The Pragmatic Programmer",
                Slug = "pragmatic-programmer-book",
                Description = "Classic programming book, excellent condition. Essential for software developers.",
                Price = 34.99m,
                DeliveryOption = DeliveryOption.Shipping,
                Condition = Condition.LikeNew,
                Status = ItemStatus.Available,
                SellerId = customer1Id,
                Location = "Auckland",
                Quantity = 1
            }, "fiction");

            // Items from Customer2
            TryAdd(new Item
            {
                Name = "Leather Jacket",
                Slug = "leather-jacket",
                Description = "Stylish leather jacket, barely worn.",
                Price = 199.99m,
                DeliveryOption = DeliveryOption.ShippingAndPickup,
                Condition = Condition.Good,
                Status = ItemStatus.Rejected,
                SellerId = customer2Id,
                Location = "Wellington",
                Quantity = 1
            }, "mens-clothing");

            TryAdd(new Item
            {
                Name = "Wooden Dining Table",
                Slug = "wooden-dining-table",
                Description = "Solid oak dining table, seats 6 people. Slight wear but very sturdy.",
                Price = 349.99m,
                DeliveryOption = DeliveryOption.Pickup,
                Condition = Condition.Fair,
                Status = ItemStatus.Available,
                SellerId = customer2Id,
                Location = "Wellington",
                Quantity = 1
            }, "tables");

            TryAdd(new Item
            {
                Name = "Complete Harry Potter Book Set",
                Slug = "harry-potter-book-set",
                Description = "All 7 books in paperback, excellent condition. A must-have for any fan.",
                Price = 89.99m,
                DeliveryOption = DeliveryOption.ShippingAndPickup,
                Condition = Condition.Good,
                Status = ItemStatus.Available,
                SellerId = customer2Id,
                Location = "Dunedin",
                Quantity = 1
            }, "fiction");

            if (demoItems.Count > 0)
            {
                await db.Items.AddRangeAsync(demoItems);
                await db.SaveChangesAsync();
            }
        }
    }
}