using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMarket.Models
{
    public enum ItemStatus
    {
        Available = 0,
        SoldOut = 1,
        Pending = 2,
        Rejected = 3
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

        [Required]
        [MaxLength(200)]
        [DisplayName("Name")]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Slug { get; set; } = null!;

        [MaxLength(4000)]
        [DisplayName("Description")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayName("Price")]
        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Price must be between 0.01 and 999999999.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        [DisplayName("Quantity")]
        public int Quantity { get; set; } = 1;

        [MaxLength(2000)]
        public string? ImageUrl { get; set; }

        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

        public ItemStatus Status { get; set; } = ItemStatus.Pending;

        [DisplayName("Condition")]
        public Condition Condition { get; set; }

        [MaxLength(500)]
        [DisplayName("Location")]
        public string? Location { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        [DisplayName("Category")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        [Required]
        [MaxLength(450)]
        public string SellerId { get; set; } = null!;

        public ApplicationUser? Seller { get; set; }

        [MaxLength(2000)]
        public string? QrCodeUrl { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }
    }
}
