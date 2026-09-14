using Microsoft.EntityFrameworkCore;
using ReMarket.DataAccess.Data;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;

namespace ReMarket.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db)
            : base(db)
        {
            _db = db;
        }

        public void Update(Category obj)
        {
            _db.Update(obj);
        }

        public bool IsSlugTaken(string slug, int? ignoreCategoryId)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            var q = _db.Categories.AsNoTracking().Where(c => c.Slug == slug);
            if (ignoreCategoryId.HasValue)
                q = q.Where(c => c.Id != ignoreCategoryId.Value);
            return q.Any();
        }
    }
}
