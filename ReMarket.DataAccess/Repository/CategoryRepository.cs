using ReMarket.Data;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;

namespace ReMarket.DataAccess.Repository
{
    /// <summary>EF Core repository for <see cref="Category"/>.</summary>
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext db)
            : base(db)
        {
        }
    }
}
