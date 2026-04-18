using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ReMarket.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Generic repository abstraction over EF Core <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/> operations.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>Returns all entities (no filter, no includes).</summary>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Returns entities optionally filtered and with related entities loaded (comma-separated navigation property names).
        /// </summary>
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);

        /// <summary>Returns the first entity matching the filter, or null.</summary>
        T Get(Expression<Func<T, bool>> filter);

        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
