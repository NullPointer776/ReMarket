using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReMarket.Data;
using ReMarket.DataAccess.Data;
using ReMarket.DataAccess.Repository;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;

namespace ReMarket
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
          
            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "category",
                pattern: "category/{slug}",
                defaults: new { area = "Admin", controller = "Category", action = "Index" });

            app.MapControllerRoute(
                name: "item",
                pattern: "item/{slug}",
                defaults: new { area = "Buyer", controller = "Item", action = "Detail" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Buyer}/{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
            using (var scope = app.Services.CreateScope())
                await SeedData.SeedAsync(scope.ServiceProvider);
            app.Run();
        }
    }
}
