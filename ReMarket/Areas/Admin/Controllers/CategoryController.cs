using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;
using ReMarket.Utility;

namespace ReMarket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
    public class CategoryController : Controller
    {
        private static readonly string[] AllowedIconExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"];
        private const long MaxIconBytes = 2 * 1024 * 1024;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;

        public CategoryController(IUnitOfWork unitOfWork, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _env = env;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category obj, IFormFile? iconFile)
        {
            ValidateIcon(iconFile, nameof(iconFile));

            if (ModelState.IsValid)
            {
                obj.Slug = EnsureSlug(obj.Slug, obj.Name, obj.ParentCategoryId, ignoreId: null);
                if (iconFile is { Length: > 0 })
                    obj.IconImagePath = await SaveIconAsync(iconFile, obj.Slug);

                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction(nameof(Index));
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category obj, IFormFile? iconFile)
        {
            if (id != obj.Id) return NotFound();

            ValidateIcon(iconFile, nameof(iconFile));

            if (ModelState.IsValid)
            {
                obj.Slug = EnsureSlug(obj.Slug, obj.Name, obj.ParentCategoryId, ignoreId: id);
                if (iconFile is { Length: > 0 })
                    obj.IconImagePath = await SaveIconAsync(iconFile, obj.Slug);

                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction(nameof(Index));
            }

            PopulateParents(obj.ParentCategoryId, excludeId: id);
            return View(obj);
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

        private void ValidateIcon(IFormFile? file, string fieldName)
        {
            if (file == null || file.Length == 0) return;
            if (file.Length > MaxIconBytes)
                ModelState.AddModelError(fieldName, "Icon must not exceed 2 MB.");
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedIconExtensions.Contains(ext))
                ModelState.AddModelError(fieldName, "Allowed icon formats: jpg, png, gif, webp, svg.");
        }

        private async Task<string> SaveIconAsync(IFormFile file, string slug)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedIconExtensions.Contains(ext))
                ext = ".png";

            var dir = Path.Combine(_env.WebRootPath, "images", "categories");
            Directory.CreateDirectory(dir);
            var name = $"{slug}{ext}";
            var path = Path.Combine(dir, name);
            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return "/images/categories/" + name;
        }
    }
}
