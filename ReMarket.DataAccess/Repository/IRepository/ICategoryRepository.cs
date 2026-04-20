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

        /// <summary>
        /// Returns true if a category other than <paramref name="ignoreCategoryId"/> already uses <paramref name="slug"/>.
        /// Uses a non-tracking query so callers can update a different instance with the same key.
        /// </summary>
        bool IsSlugTaken(string slug, int? ignoreCategoryId);
    }
}
