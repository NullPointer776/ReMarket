using Microsoft.AspNetCore.Mvc;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;

namespace ReMarket.Web.Areas.Buyer.Controllers
{
    [Area("Buyer")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var roots = _unitOfWork.Category
                .GetAll(filter: c => c.ParentCategoryId == null && c.IsActive, includeProperties: "SubCategories")
                .OrderBy(c => c.Name)
                .ToList();
            return View(roots);
        }

        public IActionResult Detail(string slug, string? search)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var category = _unitOfWork.Category
                .GetAll(filter: c => c.Slug == slug && c.IsActive, includeProperties: "SubCategories")
                .FirstOrDefault();
            if (category == null)
                return NotFound();

            var validCategoryIds = new List<int> { category.Id };
            if (category.SubCategories != null)
                validCategoryIds.AddRange(category.SubCategories.Where(s => s.IsActive).Select(s => s.Id));

            var items = _unitOfWork.Item
                .GetAll(filter: i => i.Status == ItemStatus.Available && validCategoryIds.Contains(i.CategoryId),
                        includeProperties: "Category")
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                items = items.Where(i =>
                    i.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (i.Description != null && i.Description.Contains(q, StringComparison.OrdinalIgnoreCase)));
            }

            ViewBag.Search = search;
            ViewBag.Items = items.OrderByDescending(i => i.DatePosted).ToList();
            return View(category);
        }
    }
}
