using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMarket.Models
{
    public enum Staus
    {
        Available,
        SoldOut,
        Pending,
        Rejected
    }
    public enum Condition
    {
        New,
        LikeNew,
        Good,
        Fair,
        Poor
    }
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; } = 1;
        public string ImageUrl { get; set; }

        public DateTime DatePosted { get; set; } = DateTime.Now;
        public Staus Status { get; set; }
        public Condition Condition { get; set; }
        public string Location { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string SellerId { get; set; }
        public ApplicationUser Seller { get; set; }

    }
}
