using Microsoft.EntityFrameworkCore.Diagnostics;
using ReMarket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMarket.DataAccess.Repository.IRepository
{
    public interface IItemRepository : IRepository<Item>
    {
        new void Update(Item obj);
        void Save();
    }
}
