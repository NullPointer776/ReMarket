using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReMarket.Models;

namespace ReMarket.DataAccess.Repository.IRepository
{
    /// <summary>Repository for <see cref="Category"/> entities.</summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        new void Update(Category obj);
    }
}
