using Microsoft.EntityFrameworkCore;
using ReMarket.DataAccess.Data;
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
    public class ItemRepository : Repository<Item>, IItemRepository
    {

        private ApplicationDbContext _db;
        public ItemRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<Item> GetAll(Expression<Func<Item, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<Item> query = _db.Set<Item>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return query.ToList();
        }
        public void Add(Item item)
        {
            _db.Items.Add(item);
        }
        public void Remove(Item item)
        {
            _db.Items.Remove(item);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Item obj)
        {
            _db.Update(obj);
        }
    
    }
}
