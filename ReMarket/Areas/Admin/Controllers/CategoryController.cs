using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;
using ReMarket.Utility;

namespace ReMarket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
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
                .GetAll(filter: c => c.ParentCategoryId == null, includeProperties: "SubCategories")
                .OrderBy(c => c.Name)
                .ToList();
            return View(roots);
        }

        public IActionResult Create()
        {
            PopulateParents(null);
            return View(new Category { IsActive = true });
        }

        // Ensures category name is unique under the same parent.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            var existing = _unitOfWork.Category.Get(c => c.Name == obj.Name && c.ParentCategoryId == obj.ParentCategoryId);
            if (existing != null)
            {
                ModelState.AddModelError("Name", "A category with the same name already exists.");
            }
            if (ModelState.IsValid)
            {
                obj.Slug = EnsureSlug(obj.Slug, obj.Name, obj.ParentCategoryId, ignoreId: null);
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }

            PopulateParents(obj.ParentCategoryId);
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);
            if (categoryFromDb == null) return NotFound();

            PopulateParents(categoryFromDb.ParentCategoryId, excludeId: id);
            return View(categoryFromDb);
        }

        // Same duplicate-name check as Create, except for this category id.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Edit(int id, Category obj)
        {
            if (id != obj.Id) return Task.FromResult<IActionResult>(NotFound());

            var duplicate = _unitOfWork.Category.Get(c =>
                c.Name == obj.Name
                && c.ParentCategoryId == obj.ParentCategoryId
                && c.Id != obj.Id);
            if (duplicate != null)
            {
                ModelState.AddModelError(nameof(Category.Name), "A category with the same name already exists.");
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return Task.FromResult<IActionResult>(RedirectToAction(nameof(Index)));
            }

            PopulateParents(obj.ParentCategoryId, excludeId: id);
            return Task.FromResult<IActionResult>(View(obj));
        }

        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.Category.Get(u => u.Id == id);
            if (obj == null) return NotFound();
            return View(obj);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int id)
        {
            var obj = _unitOfWork.Category.Get(u => u.Id == id);
            if (obj == null) return NotFound();

            if (_unitOfWork.Category.GetAll(filter: c => c.ParentCategoryId == id).Any())
            {
                TempData["error"] = "Cannot delete a category with subcategories.";
                return RedirectToAction(nameof(Index));
            }
            if (_unitOfWork.Item.GetAll(filter: i => i.CategoryId == id).Any())
            {
                TempData["error"] = "Cannot delete a category that already has items.";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateParents(int? selected, int? excludeId = null)
        {
            var roots = _unitOfWork.Category
                .GetAll(filter: c => c.ParentCategoryId == null && (excludeId == null || c.Id != excludeId.Value))
                .OrderBy(c => c.Name);
            ViewBag.ParentCategories = new SelectList(roots, "Id", "Name", selected);
        }

        private string EnsureSlug(string? provided, string name, int? parentId, int? ignoreId)
        {
            string baseSlug;
            if (!string.IsNullOrWhiteSpace(provided))
            {
                baseSlug = SlugHelper.ToSlug(provided);
            }
            else
            {
                baseSlug = SlugHelper.ToSlug(name);
                if (parentId.HasValue)
                {
                    var parent = _unitOfWork.Category.Get(c => c.Id == parentId);
                    if (parent?.Slug != null)
                        baseSlug = $"{parent.Slug}-{baseSlug}";
                }
            }
            var candidate = baseSlug;
            var n = 1;
            while (true)
            {
                if (!_unitOfWork.Category.IsSlugTaken(candidate, ignoreId))
                    return candidate;
                candidate = $"{baseSlug}-{n++}";
            }
        }
    }
}
