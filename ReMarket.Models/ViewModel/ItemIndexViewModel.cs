using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMarket.Models.ViewModel
{
    public class ItemIndexViewModel
    {
        public IEnumerable<Item> Items { get; set; } = new List<Item>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public string? Search { get; set; }
        public int? CategoryId { get; set; }

        // For filter and sort options
        public string? Condition { get; set; }
        public string? Location { get; set; }
        public string? DeliveryOption { get; set; }
        // "newest", "oldest", "price_high", "price_low"
        public string? SortBy { get; set; }  
        public string? ActiveTab { get; set; }
    }
}