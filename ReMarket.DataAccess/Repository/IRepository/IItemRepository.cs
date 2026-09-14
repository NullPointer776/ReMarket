using ReMarket.Models;

namespace ReMarket.DataAccess.Repository.IRepository
{
    public interface IItemRepository : IRepository<Item>
    {
        new void Update(Item obj);
    }
}
