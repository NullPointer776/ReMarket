using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [MaxLength(200)]
        [DisplayName("Item Name")]
        public string Name { get; set; } = string.Empty;   

        [MaxLength(1000)]
        [DisplayName("Item Description")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; } 

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

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [DisplayName("Condition")]
        public Condition Condition { get; set; } = Condition.Good;

        [Required]
        [MaxLength(500)]
        [DisplayName("Item current Location")]
        public string Location { get; set; } = string.Empty;

        [DisplayName("Delivery Option")]
        public DeliveryOption DeliveryOption { get; set; } = DeliveryOption.Pickup;

        public string? ImageUrl { get; set; }

        /// <summary>JSON array of image URLs for images 2–8 (the first is always <see cref="ImageUrl"/>).</summary>
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
