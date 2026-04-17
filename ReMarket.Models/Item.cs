using System;
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
        public string Name { get; set; } = null!;

        [MaxLength(4000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Price must be between 0.01 and 999999999.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [MaxLength(2000)]
        public string? ImageUrl { get; set; }

        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

        public ItemStatus Status { get; set; } = ItemStatus.Pending;

        public Condition Condition { get; set; }

        [MaxLength(500)]
        public string? Location { get; set; }

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
