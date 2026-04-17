using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMarket.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Display(Name ="First Name")]
        public string? FirstName { get; set; }
        [PersonalData]
        [Display(Name ="Last Name")]
        public string? LastName { get; set; }
        [PersonalData]
        [Display(Name ="StreetAddress")]
        public string? StreetAddress { get; set; }
        [PersonalData]
        [Display(Name = "Suburb")]
        public string? Suburb { get; set; }
        [PersonalData]
        [Display(Name = "City")]
        public string? City { get; set; }
        [PersonalData]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }
        [PersonalData]
        [Display(Name = "Country")]
        public string? Country { get; set; }
        public ICollection<Item> ItemsListed { get; set; } = new List<Item>();
    }
}
