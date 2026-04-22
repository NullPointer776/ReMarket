using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMarket.Models.ViewModel
{
    public class ItemDetailViewModel
    {
        public Item Item { get; set; } = null!;
        public string SellerName { get; set; } = string.Empty;
        public string SellerEmail { get; set; } = string.Empty;
    }
}
