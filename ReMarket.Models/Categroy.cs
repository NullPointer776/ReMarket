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

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Category Description")]
        public string? Description { get; set; }


        [MaxLength(120)]
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
