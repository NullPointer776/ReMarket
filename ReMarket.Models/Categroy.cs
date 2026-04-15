using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMarket.Models
{
    public class Category
    {
        // Primary key
        [Key]
        public int Id { get; set; }
        // Required category name with a maximum length of 100 characters
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        // Optional category description with a maximum length of 500 characters
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        // Indicates whether the category is active or not, defaulting to true
        public bool IsActive { get; set; } = true;

        // Slug is a URL-friendly version of the category name, used for SEO and routing purposes
        public string Slug { get; set; }
        // Foreign key to the parent category, allowing for hierarchical categorization
        [ForeignKey("ParentCategoryId")]
        public int? ParentCategoryId { get; set; }
        // Navigation property to the parent category, enabling access to the parent category's details
        public Category ParentCategory { get; set; }
        // Navigation properties for related subcategories and items
        public ICollection<Category> SubCategories { get; set; }
       
        public ICollection<Item> Items { get; set; }
    }
}
