using System.ComponentModel.DataAnnotations;
using ReMarket.Models.Validation;

namespace ReMarket.Models.ViewModel
{
    public class RejectItemViewModel
    {
        public int Id { get; set; }

        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a rejection reason.")]
        [StringLength(500, MinimumLength = 1)]
        [SafeText]
        [Display(Name = "Rejection reason")]
        [DataType(DataType.MultilineText)]
        public string? RejectionReason { get; set; }
    }
}
