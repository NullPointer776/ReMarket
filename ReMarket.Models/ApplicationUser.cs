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
        [Required]
        public string FirstName { get; set; }
        [PersonalData]
        [Required]
        public string LastName { get; set; }
        [PersonalData]
        [Required]
        public string Email { get; set; }
        [PersonalData]
        [Required]
        public string PhoneNumber { get; set; }
        [PersonalData]
        [Required]
        public string StreetAddress { get; set; }
        [PersonalData]
        [Required]
        public string Suburb { get; set; }
        [PersonalData]
        [Required]
        public string City { get; set; }
        [PersonalData]
        [Required]
        public string PostalCode { get; set; }
        [PersonalData]
        [Required]
        public string Country { get; set; }
        public ICollection<Item> ItemsListed { get; set; }
    }
}
