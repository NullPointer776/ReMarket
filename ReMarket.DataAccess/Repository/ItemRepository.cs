using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReMarket.Data;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;

namespace ReMarket.DataAccess.Repository
{
    /// <summary>EF Core repository for <see cref="Item"/>.</summary>
    public class ItemRepository : Repository<Item>, IItemRepository
    {
        public ItemRepository(ApplicationDbContext db)
            : base(db)
        {
        }
    }
}
