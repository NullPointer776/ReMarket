using ReMarket.DataAccess.Repository.IRepository;

namespace ReMarket.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IItemRepository Item { get; }
        void Save();
    }
}
