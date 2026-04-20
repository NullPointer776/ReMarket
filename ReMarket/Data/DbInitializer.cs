using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ReMarket.Data;
using ReMarket.Models;
using ReMarket.Utility;

namespace ReMarket.Web.Data
{
    /// <summary>
    /// Applies pending migrations, seeds roles, optionally creates the default admin from configuration,
    /// and backfills Buyer + Seller roles with set-based SQL (constant startup cost vs. user count).
    /// Admin password reset and lockout clearing run only in Development; production never overwrites credentials or lockout.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>Squashed baseline migration id (must match <c>addSeedDataMigration</c> class attribute).</summary>
        private const string BaselineSchemaMigrationId = "20260419004145_addSeedDataMigration";

        /// <summary>EF Core product version written to <c>__EFMigrationsHistory</c> for that migration.</summary>
        private const string EfCoreProductVersion = "9.0.2";

        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var normalizer = scope.ServiceProvider.GetRequiredService<ILookupNormalizer>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            var adminEmail = configuration["Seed:AdminEmail"];
            if (string.IsNullOrWhiteSpace(adminEmail))
                adminEmail = SD.DefaultAdminEmail;
            var adminPassword = configuration["Seed:AdminPassword"];

            await RecordSquashedBaselineIfDatabaseAlreadyExistsAsync(db, logger);

            if ((await db.Database.GetPendingMigrationsAsync()).Any())
            {
                await db.Database.MigrateAsync();
            }

            foreach (var role in new[] { SD.Role_Admin, SD.Role_Seller, SD.Role_Buyer })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var normalizedAdminEmail = normalizer.NormalizeEmail(adminEmail) ?? string.Empty;

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                if (string.IsNullOrWhiteSpace(adminPassword))
                {
                    logger.LogWarning(
                        "Seed:AdminPassword is not configured; skipping default admin user creation for {Email}.",
                        adminEmail);
                }
                else
                {
                    admin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        LockoutEnabled = false,
                        FirstName = "Site",
                        LastName = "Admin"
                    };
                    var create = await userManager.CreateAsync(admin, adminPassword);
                    if (!create.Succeeded)
                    {
                        foreach (var err in create.Errors)
                            logger.LogError("Seed admin failed: {Code} {Description}", err.Code, err.Description);
                        return;
                    }

                    await userManager.AddToRoleAsync(admin, SD.Role_Admin);
                    logger.LogInformation("Seeded default admin {Email}.", adminEmail);
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(admin, SD.Role_Admin))
                    await userManager.AddToRoleAsync(admin, SD.Role_Admin);

                if (env.IsDevelopment())
                {
                    var needsUpdate = false;
                    if (!admin.EmailConfirmed)
                    {
                        admin.EmailConfirmed = true;
                        needsUpdate = true;
                    }

                    if (admin.LockoutEnd != null)
                    {
                        admin.LockoutEnd = null;
                        needsUpdate = true;
                    }

                    if (needsUpdate)
                        await userManager.UpdateAsync(admin);

                    if (!string.IsNullOrWhiteSpace(adminPassword))
                    {
                        var token = await userManager.GeneratePasswordResetTokenAsync(admin);
                        var reset = await userManager.ResetPasswordAsync(admin, token, adminPassword);
                        if (!reset.Succeeded)
                        {
                            foreach (var err in reset.Errors)
                                logger.LogError("Reset admin password failed: {Code} {Description}", err.Code, err.Description);
                        }
                    }
                }
                else
                {
                    if (!admin.EmailConfirmed)
                    {
                        admin.EmailConfirmed = true;
                        await userManager.UpdateAsync(admin);
                    }
                }
            }

            await BackfillBuyerAndSellerRolesAsync(db, normalizedAdminEmail, logger);
        }

        /// <summary>
        /// When the database already has Identity tables from a prior migration chain that is no longer in this assembly,
        /// <c>20260419004145_addSeedDataMigration</c> is still "pending" and its <c>Up</c> would try to recreate <c>AspNetRoles</c>.
        /// This records that migration as applied without running its DDL so EF Core can apply only newer migrations.
        /// </summary>
        private static async Task RecordSquashedBaselineIfDatabaseAlreadyExistsAsync(ApplicationDbContext db, ILogger logger)
        {
            try
            {
                var rows = await db.Database.ExecuteSqlInterpolatedAsync($@"
IF OBJECT_ID(N'[dbo].[AspNetRoles]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {BaselineSchemaMigrationId})
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ({BaselineSchemaMigrationId}, {EfCoreProductVersion});
END
");
                if (rows > 0)
                    logger.LogInformation(
                        "Recorded {MigrationId} in __EFMigrationsHistory because the schema already exists (squashed migration baseline).",
                        BaselineSchemaMigrationId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not auto-record squashed baseline migration; Migrate may fail on duplicate objects.");
            }
        }

        /// <summary>
        /// Inserts missing AspNetUserRoles rows in two round-trips without loading users into memory.
        /// </summary>
        private static async Task BackfillBuyerAndSellerRolesAsync(
            ApplicationDbContext db,
            string normalizedAdminEmail,
            ILogger logger)
        {
            var buyerRoleId = await db.Set<IdentityRole>()
                .AsNoTracking()
                .Where(r => r.Name == SD.Role_Buyer)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();
            var sellerRoleId = await db.Set<IdentityRole>()
                .AsNoTracking()
                .Where(r => r.Name == SD.Role_Seller)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(buyerRoleId) || string.IsNullOrEmpty(sellerRoleId))
            {
                logger.LogError("Buyer or Seller role is missing from the database; cannot backfill user roles.");
                return;
            }

            var rowsAffected = 0;
            foreach (var roleId in new[] { buyerRoleId, sellerRoleId })
            {
                rowsAffected += await db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, {roleId}
FROM AspNetUsers u
WHERE (u.NormalizedEmail IS NULL OR u.NormalizedEmail <> {normalizedAdminEmail})
AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles x WHERE x.UserId = u.Id AND x.RoleId = {roleId})
");
            }

            if (rowsAffected > 0)
                logger.LogInformation("Backfilled Buyer/Seller role rows: {Rows} total.", rowsAffected);
        }
    }
}
