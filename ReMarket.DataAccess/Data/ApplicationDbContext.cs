using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReMarket.Models;

namespace ReMarket.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Item> Items { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Slug).HasMaxLength(120);
                entity.HasIndex(e => e.Slug).IsUnique().HasFilter("[Slug] IS NOT NULL");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasOne(e => e.ParentCategory).WithMany(e => e.SubCategories)
                      .HasForeignKey(e => e.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Items).WithOne(e => e.Category)
                      .HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Slug).HasMaxLength(200);
                entity.HasIndex(e => e.Slug).IsUnique().HasFilter("[Slug] IS NOT NULL");
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ImageUrl).HasMaxLength(2000);
                entity.Property(e => e.MoreImageUrlsJson).HasMaxLength(8000);
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.QrCodeUrl).HasMaxLength(2000);
                entity.Property(e => e.RejectionReason).HasMaxLength(1000);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Condition).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.DeliveryOption).HasConversion<string>().HasMaxLength(20);
                entity.HasOne(e => e.Seller).WithMany(e => e.ItemsListed)
                      .HasForeignKey(e => e.SellerId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.StreetAddress).HasMaxLength(200);
                entity.Property(e => e.Suburb).HasMaxLength(100);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.Country).HasMaxLength(100);
            });

            // Seed Categories 
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", Description = "Gadgets and devices", Slug = "electronics" },
                new Category { Id = 2, Name = "Furniture", Description = "Home and office furniture", Slug = "furniture" },
                new Category { Id = 3, Name = "Clothing", Description = "Apparel and accessories", Slug = "clothing" },
                new Category { Id = 4, Name = "Mobile Phones", Description = "Smartphones and accessories", Slug = "mobile-phones", ParentCategoryId = 1 },
                new Category { Id = 5, Name = "Laptops", Description = "Notebooks and accessories", Slug = "laptops", ParentCategoryId = 1 },
                new Category { Id = 6, Name = "Sofas", Description = "Comfortable seating", Slug = "sofas", ParentCategoryId = 2 },
                new Category { Id = 7, Name = "Tables", Description = "Dining and office tables", Slug = "tables", ParentCategoryId = 2 },
                new Category { Id = 8, Name = "Food & Beverages", Description = "Groceries and drinks", Slug = "food-beverages" },  
                new Category { Id = 9, Name = "Books", Description = "Fiction and non-fiction books", Slug = "books" },
                new Category { Id = 10, Name = "Toys & Games", Description = "Toys and board games", Slug = "toys-games" }
            );
        }
    }
}