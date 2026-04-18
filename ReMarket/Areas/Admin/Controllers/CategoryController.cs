using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;

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

        public IActionResult Index(string? slug = null)
        {

            List<Category> rootCategories = _unitOfWork.Category
                .GetAll(filter: c => c.ParentCategoryId == null,
                        includeProperties: "SubCategories")
                .ToList();
            return View(rootCategories);
        }

        public IActionResult Create()
        {
            ViewBag.ParentCategories = new SelectList(
                _unitOfWork.Category.GetAll(filter: c => c.ParentCategoryId == null),
                "Id", "Name"
            );
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(obj.Slug))
                {
                    if (obj.ParentCategoryId.HasValue)
                    {
                        var parent = _unitOfWork.Category.Get(c => c.Id == obj.ParentCategoryId);
                        obj.Slug = $"{parent.Slug}-{obj.Name.ToLower().Replace(" ", "-")}";
                    }
                    else
                    {
                        obj.Slug = obj.Name.ToLower().Replace(" ", "-");
                    }

                }

                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);
            if (categoryFromDb == null) return NotFound();

            ViewBag.ParentCategories = new SelectList(
                _unitOfWork.Category.GetAll(filter: c => c.ParentCategoryId == null && c.Id != id),
                "Id", "Name"
            );
            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(int id, Category obj)
        {
            if (id != obj.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Delete(int id)
        {
            Category? obj = _unitOfWork.Category.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int id)
        {
            Category? obj = _unitOfWork.Category.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            if (_unitOfWork.Category
                .GetAll(filter: c => c.ParentCategoryId == id)
                .Any()) 
            { 
                TempData["error"] = "Cannot delete a category with subcategories."; 
                return RedirectToAction("Index"); 
            }
            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}