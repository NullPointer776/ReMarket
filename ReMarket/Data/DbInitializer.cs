using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ReMarket.Data;
using ReMarket.Models;
using ReMarket.Utility;

namespace ReMarket.Web.Data
{
    /// <summary>
    /// Applies pending migrations and seeds roles and a default administrator account.
    /// Also backfills Buyer + Seller roles on every non-admin user so any account can both buy and sell.
    /// Admin password reset and lockout clearing run only in Development; production never overwrites credentials or lockout.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

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

            var admin = await userManager.FindByEmailAsync(SD.DefaultAdminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = SD.DefaultAdminEmail,
                    Email = SD.DefaultAdminEmail,
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    FirstName = "Site",
                    LastName = "Admin"
                };
                var create = await userManager.CreateAsync(admin, SD.DefaultAdminPassword);
                if (!create.Succeeded)
                {
                    foreach (var err in create.Errors)
                        logger.LogError("Seed admin failed: {Code} {Description}", err.Code, err.Description);
                    return;
                }
                await userManager.AddToRoleAsync(admin, SD.Role_Admin);
                logger.LogInformation("Seeded default admin {Email}.", SD.DefaultAdminEmail);
            }
            else
            {
                if (!await userManager.IsInRoleAsync(admin, SD.Role_Admin))
                    await userManager.AddToRoleAsync(admin, SD.Role_Admin);

                if (env.IsDevelopment())
                {
                    // Development only: predictable local admin and recovery from lockout / unconfirmed email.
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

                    var token = await userManager.GeneratePasswordResetTokenAsync(admin);
                    var reset = await userManager.ResetPasswordAsync(admin, token, SD.DefaultAdminPassword);
                    if (!reset.Succeeded)
                    {
                        foreach (var err in reset.Errors)
                            logger.LogError("Reset admin password failed: {Code} {Description}", err.Code, err.Description);
                    }
                }
                else
                {
                    // Production / staging: do not reset password, clear lockout, or weaken security on every startup.
                    if (!admin.EmailConfirmed)
                    {
                        admin.EmailConfirmed = true;
                        await userManager.UpdateAsync(admin);
                    }
                }
            }

            var allUsers = userManager.Users.ToList();
            foreach (var user in allUsers)
            {
                if (string.Equals(user.Email, SD.DefaultAdminEmail, StringComparison.OrdinalIgnoreCase))
                    continue;

                var roles = await userManager.GetRolesAsync(user);
                if (!roles.Contains(SD.Role_Buyer))
                    await userManager.AddToRoleAsync(user, SD.Role_Buyer);
                if (!roles.Contains(SD.Role_Seller))
                    await userManager.AddToRoleAsync(user, SD.Role_Seller);
            }
        }
    }
}
