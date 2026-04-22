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

        /// <summary>Distinct image URLs in display order (1–8).</summary>
        public IReadOnlyList<string> ImageUrls { get; set; } = System.Array.Empty<string>();

        public bool CanAddMoreImages { get; set; }

        /// <summary>Link for the seller to add more images; null if not allowed.</summary>
        public string? AddMoreImagesUrl { get; set; }
    }

    public class RejectItemViewModel
    {
        public int Id { get; set; }

        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a rejection reason.")]
        [MaxLength(500)]
        [Display(Name = "Rejection reason")]
        [DataType(DataType.MultilineText)]
        public string? RejectionReason { get; set; }
    }
}
