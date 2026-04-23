using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public IReadOnlyList<string> ImageUrls { get; set; } = System.Array.Empty<string>();
        public bool IsListingOwner { get; set; }
        public bool CanAddMoreImages { get; set; }
        public string? AddMoreImagesUrl { get; set; }
    }

}
