using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMarket.Models.Validation;

namespace ReMarket.Models
{
    public enum ItemStatus
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
    public enum DeliveryOption
    {
        Shipping,
        FreeShipping,
        Pickup,
        ShippingAndPickup
    }
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        [SafeText]
        [DisplayName("Item Name")]
        public string Name { get; set; } = string.Empty;   

        [StringLength(1000)]
        [SafeText]
        [DisplayName("Item Description")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; } 

        [StringLength(200)]
        [RegularExpression(@"^[a-z0-9\-]*$", ErrorMessage = "Slug may only contain lowercase letters, numbers, and hyphens.")]
        [DisplayName("URL Slug")]
        public string? Slug { get; set; }

        [Required]
        [DisplayName("Price")]
        [Range(0.01, 999999999, ErrorMessage = "Price must be between 0.01 and 999999999.")]
        public decimal Price { get; set; }

        [Range(1, 999999999, ErrorMessage = "Please enter a quantity between 1 and 999999999.")]
        [DisplayName("Quantity number of the item")]
        public int Quantity { get; set; } = 1;

        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

        [DisplayName("Item auditing Status")]
        public ItemStatus Status { get; set; } = ItemStatus.Pending;

        [StringLength(500)]
        [SafeText]
        public string? RejectionReason { get; set; }

        [DisplayName("Condition")]
        public Condition Condition { get; set; } = Condition.Good;

        [Required]
        [StringLength(500, MinimumLength = 1)]
        [SafeText]
        [DisplayName("Item current Location")]
        public string Location { get; set; } = string.Empty;

        [DisplayName("Delivery Option")]
        public DeliveryOption DeliveryOption { get; set; } = DeliveryOption.Pickup;

        public string? ImageUrl { get; set; }

        // JSON list of image URLs after the first (ImageUrl is the first one).
        [MaxLength(8000)]
        public string? MoreImageUrlsJson { get; set; }

        public string? QrCodeUrl { get; set; }

        [Required]
        [DisplayName("Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]    
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Required]
        public string SellerId { get; set; } = string.Empty;
        [ForeignKey("SellerId")]
        public ApplicationUser? Seller { get; set; }    
    }
}
