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
            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (await userManager.FindByEmailAsync("test@2test.com") != null) return;

            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@2test.com",
                NormalizedUserName = "TESTUSER",
                NormalizedEmail = "TEST@2TEST.COM",
            };

            await userManager.CreateAsync(user, "Test@123");

            await db.Items.AddRangeAsync(
                new Item { Name = "iPhone 12", Slug = "iphone-12", Description = "A used iPhone 12 in good condition.", Price = 499.99m, Condition = Condition.Good, Status = ItemStatus.Pending, CategoryId = 1, SellerId = user.Id },
                new Item { Name = "Office Chair", Slug = "office-chair", Description = "Ergonomic office chair with adjustable height.", Price = 149.99m, Condition = Condition.Good, Status = ItemStatus.Available, CategoryId = 2, SellerId = user.Id },
                new Item { Name = "Leather Jacket", Slug = "leather-jacket", Description = "Stylish leather jacket, barely worn.", Price = 199.99m, Condition = Condition.Good, Status = ItemStatus.Rejected, CategoryId = 3, SellerId = user.Id }
            );

            await db.SaveChangesAsync();
        }
    }
}
