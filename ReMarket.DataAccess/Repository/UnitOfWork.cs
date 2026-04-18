using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReMarket.Data;
using ReMarket.DataAccess.Repository.IRepository;

namespace ReMarket.DataAccess.Repository
{
    /// <summary>
    /// Default <see cref="IUnitOfWork"/> implementation sharing one <see cref="ApplicationDbContext"/> per request.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        public ICategoryRepository Category { get; private set; }
        public IItemRepository Item { get; private set; }

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
