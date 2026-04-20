using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReMarket.Data;
using ReMarket.Models;
using ReMarket.Utility;

namespace ReMarket.Web.Data
{
    /// <summary>
    /// Runs EF migrations, ensures Identity roles exist, and gives every user Buyer + Seller roles if missing.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            const string baselineMigration = "20260419004145_addSeedDataMigration";
            const string efProductVersion = "9.0.2";

            // If the DB was built with old migrations, mark the squashed baseline so Migrate does not recreate AspNetRoles.
            await db.Database.ExecuteSqlInterpolatedAsync($@"
IF OBJECT_ID(N'[dbo].[AspNetRoles]', N'U') IS NOT NULL
 AND OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {baselineMigration})
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ({baselineMigration}, {efProductVersion});
END");

            if ((await db.Database.GetPendingMigrationsAsync()).Any())
                await db.Database.MigrateAsync();

            foreach (var role in new[] { SD.Role_Admin, SD.Role_Seller, SD.Role_Buyer })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var buyerId = await db.Set<IdentityRole>().AsNoTracking()
                .Where(r => r.Name == SD.Role_Buyer).Select(r => r.Id).FirstOrDefaultAsync();
            var sellerId = await db.Set<IdentityRole>().AsNoTracking()
                .Where(r => r.Name == SD.Role_Seller).Select(r => r.Id).FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(buyerId) || string.IsNullOrEmpty(sellerId))
                return;

            foreach (var roleId in new[] { buyerId, sellerId })
            {
                await db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, {roleId} FROM AspNetUsers u
WHERE NOT EXISTS (SELECT 1 FROM AspNetUserRoles x WHERE x.UserId = u.Id AND x.RoleId = {roleId})");
            }
        }
    }
}
