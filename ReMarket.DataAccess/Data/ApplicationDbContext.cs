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
        public DbSet<ShoppingCart> ShoppingCarts { get; set; } = null!;
        public DbSet<OrderHeader> OrderHeaders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;

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

            builder.Entity<ShoppingCart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Item)
                      .WithMany()
                      .HasForeignKey(e => e.ItemId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ApplicationUser)
                      .WithMany()
                      .HasForeignKey(e => e.ApplicationUserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.ApplicationUserId, e.ItemId }).IsUnique();
            });

            builder.Entity<OrderHeader>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderTotal).HasPrecision(18, 2);
                entity.HasOne(e => e.ApplicationUser)
                      .WithMany()
                      .HasForeignKey(e => e.ApplicationUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.HasOne(e => e.OrderHeader)
                      .WithMany()
                      .HasForeignKey(e => e.OrderHeaderId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Item)
                      .WithMany()
                      .HasForeignKey(e => e.ItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.StreetAddress).HasMaxLength(200);
                entity.Property(e => e.Suburb).HasMaxLength(100);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
              
            });
           
        }
    }
}