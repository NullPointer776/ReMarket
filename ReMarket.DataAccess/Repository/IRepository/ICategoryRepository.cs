using ReMarket.Models;

namespace ReMarket.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        new void Update(Category obj);

        bool IsSlugTaken(string slug, int? ignoreCategoryId);
    }
}
