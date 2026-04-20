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
        [Display(Name = "Category Name")]
        public string Name { get; set; } = null!;
        // Optional category description with a maximum length of 500 characters
        [MaxLength(500)]
        public string? Description { get; set; } = string.Empty;

        // Slug is a URL-friendly version of the category name, used for SEO and routing purposes
        [MaxLength(120)]
        public string? Slug { get; set; }

        // Whether the category is visible to buyers on the public site.
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        // Foreign key to the parent category, allowing for hierarchical categorization
        [ForeignKey("ParentCategoryId")]
        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }
        // Navigation property to the parent category, enabling access to the parent category's details
        public Category? ParentCategory { get; set; }
        // Navigation properties for related subcategories and items
        public ICollection<Category>? SubCategories { get; set; }

        public ICollection<Item>? Items { get; set; }
    }
}
