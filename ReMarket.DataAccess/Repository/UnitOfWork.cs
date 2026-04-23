using ReMarket.DataAccess.Data;
using ReMarket.DataAccess.Repository.IRepository;

namespace ReMarket.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public ICategoryRepository Category { get; }
        public IItemRepository Item { get; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Item = new ItemRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}