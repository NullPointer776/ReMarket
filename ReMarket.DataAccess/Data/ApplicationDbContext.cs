
using Microsoft.EntityFrameworkCore;
using ReMarket.Models;

namespace ReMarket.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

       public DbSet<Category> Categories { get; set; }
       public DbSet<Item> Items { get; set; }


    }
}
