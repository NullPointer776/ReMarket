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
            var tracked = _db.Items.Find(obj.Id);
            if (tracked == null)
            {
                _db.Update(obj);
                return;
            }

            tracked.Name = obj.Name;
            tracked.Description = obj.Description;
            tracked.Slug = obj.Slug;
            tracked.Price = obj.Price;
            tracked.Quantity = obj.Quantity;
            tracked.DatePosted = obj.DatePosted;
            tracked.Status = obj.Status;
            tracked.RejectionReason = obj.RejectionReason;
            tracked.Condition = obj.Condition;
            tracked.Location = obj.Location;
            tracked.DeliveryOption = obj.DeliveryOption;
            tracked.ImageUrl = obj.ImageUrl;
            tracked.MoreImageUrlsJson = obj.MoreImageUrlsJson;
            tracked.QrCodeUrl = obj.QrCodeUrl;
            tracked.CategoryId = obj.CategoryId;
            tracked.SellerId = obj.SellerId;
        }
    
    }
}
