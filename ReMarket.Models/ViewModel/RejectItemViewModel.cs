using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMarket.Models.ViewModel
{
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
