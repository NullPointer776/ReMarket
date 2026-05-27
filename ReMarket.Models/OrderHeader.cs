using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ReMarket.Models.Validation;

namespace ReMarket.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderTotal { get; set; }

        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateOnly PaymentDueDate { get; set; }
        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        [SafeText]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(30, MinimumLength = 6)]
        [PhoneNumber]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(300, MinimumLength = 1)]
        [SafeText]
        public string StreetAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        [SafeText]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        [SafeText]
        public string State { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;
    }
}
