using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMarket.Models.Validation;

namespace ReMarket.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        [SafeText]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [SafeText]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Category Description")]
        public string? Description { get; set; }

        [StringLength(120)]
        [RegularExpression(@"^[a-z0-9\-]*$", ErrorMessage = "Slug may only contain lowercase letters, numbers, and hyphens.")]
        [Display(Name = "URL Slug")]
        public string? Slug { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public Category? ParentCategory { get; set; }

        public ICollection<Category>? SubCategories { get; set; }

        public ICollection<Item>? Items { get; set; }
    }
}
