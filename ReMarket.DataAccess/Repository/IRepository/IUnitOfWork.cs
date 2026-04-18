namespace ReMarket.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Coordinates repositories and persists changes in a single unit of work.
    /// </summary>
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IItemRepository Item { get; }

        /// <summary>Saves all changes made in this context to the database.</summary>
        void Save();
    }
}
