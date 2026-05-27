using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMarket.Models.Validation;

namespace ReMarket.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [StringLength(100)]
        [SafeText]
        [Display(Name ="First Name")]
        public string? FirstName { get; set; }
        [PersonalData]
        [StringLength(100)]
        [SafeText]
        [Display(Name ="Last Name")]
        public string? LastName { get; set; }

        [PersonalData]
        [StringLength(300)]
        [SafeText]
        [Display(Name ="StreetAddress")]
        public string? StreetAddress { get; set; }
        [PersonalData]
        [StringLength(100)]
        [SafeText]
        [Display(Name = "Suburb")]
        public string? Suburb { get; set; }
        [PersonalData]
        [StringLength(100)]
        [SafeText]
        [Display(Name = "City")]
        public string? City { get; set; }
        [PersonalData]
        [StringLength(20)]
        [PostalCode]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }
        [PersonalData]
        [StringLength(100)]
        [SafeText]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [PersonalData]
        [StringLength(100)]
        [SafeText]
        [Display(Name = "State / Region")]
        public string? State { get; set; }

        public ICollection<Item> ItemsListed { get; set; } = new List<Item>();
    }
}
