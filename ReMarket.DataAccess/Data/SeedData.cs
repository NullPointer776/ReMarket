using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ReMarket.Data;
using ReMarket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMarket.DataAccess.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Create roles
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!await roleManager.RoleExistsAsync("Customer"))
                await roleManager.CreateAsync(new IdentityRole("Customer"));

            // Create Admin user
            if (await userManager.FindByEmailAsync("admin@remarket.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@remarket.com",
                    Email = "admin@remarket.com",
                    NormalizedUserName = "ADMIN@REMARKET.COM",
                    NormalizedEmail = "ADMIN@REMARKET.COM",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Create Customer user
            if (await userManager.FindByEmailAsync("customer@remarket.com") == null)
            {
                var customer = new ApplicationUser
                {
                    UserName = "customer@remarket.com",
                    Email = "customer@remarket.com",
                    NormalizedUserName = "CUSTOMER@REMARKET.COM",
                    NormalizedEmail = "CUSTOMER@REMARKET.COM",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(customer, "Customer@123");
                await userManager.AddToRoleAsync(customer, "Customer");
                await db.Items.AddRangeAsync(
                    new Item
                    {
                        Name = "iPhone 12",
                        Slug = "iphone-12",
                        Description = "A used iPhone 12 in good condition.",
                        Price = 499.99m,
                        DeliveryOption = DeliveryOption.Pickup,
                        Condition = Condition.Good,
                        Status = ItemStatus.Pending,
                        CategoryId = 1,
                        SellerId = customer.Id,
                        Location = "Auckland CBD"     
                      },
                    new Item
                    {
                        Name = "Office Chair",
                        Slug = "office-chair",
                        Description = "Ergonomic office chair with adjustable height.",
                        Price = 149.99m,
                        DeliveryOption = DeliveryOption.Shipping,
                        Condition = Condition.New,
                        Status = ItemStatus.Available,
                        CategoryId = 2,
                        SellerId = customer.Id,
                        Location = "Wellington",
                        Quantity = 2
                    },
                    new Item
                    {
                        Name = "Leather Jacket",
                        Slug = "leather-jacket",
                        Description = "Stylish leather jacket, barely worn.",
                        Price = 199.99m,
                        DeliveryOption = DeliveryOption.ShippingAndPickup,
                        Condition = Condition.Good,
                        Status = ItemStatus.Rejected,
                        CategoryId = 3,
                        SellerId = customer.Id,
                        Location = "Wellington",
                        Quantity = 1
                      }
                );


                await db.SaveChangesAsync();
            }
        }
    }
}
