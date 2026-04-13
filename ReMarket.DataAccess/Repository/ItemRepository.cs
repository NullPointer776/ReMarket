using ReMarket.Data;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ReMarket.DataAccess.Repository
{
    public class ItemRepository : IItemRepository
    {
        private ApplicationDbContext _db;
        public ItemRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        void IRepository<Item>.Add(Item entity)
        {
            throw new NotImplementedException();
        }

        Item IRepository<Item>.Get(Expression<Func<Item, bool>> filter)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Item> IRepository<Item>.GetAll()
        {
            throw new NotImplementedException();
        }

        void IRepository<Item>.Remove(Item entity)
        {
            throw new NotImplementedException();
        }

        void IRepository<Item>.RemoveRange(IEnumerable<Item> entities)
        {
            throw new NotImplementedException();
        }

        void IRepository<Item>.Update(Item entity)
        {
            throw new NotImplementedException();
        }
    }
}
