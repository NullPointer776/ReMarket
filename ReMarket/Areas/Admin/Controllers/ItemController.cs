using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;
using ReMarket.Utility;

namespace ReMarket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public class ItemController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;

        public ItemController(IUnitOfWork unitOfWork, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _env = env;
        }

        /// <summary>
        /// Admin dashboard listing every item with optional status/keyword filters.
        /// </summary>
        public IActionResult Index(ItemStatus? status, string? search)
        {
            var items = _unitOfWork.Item
                .GetAll(includeProperties: "Category,Seller")
                .AsEnumerable();

            if (status.HasValue)
                items = items.Where(i => i.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                items = items.Where(i =>
                    i.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (i.Seller != null && (i.Seller.Email ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)));
            }

            ViewBag.Status = status;
            ViewBag.Search = search;
            ViewBag.PendingCount = _unitOfWork.Item.GetAll(filter: i => i.Status == ItemStatus.Pending).Count();

            return View(items.OrderByDescending(i => i.DatePosted).ToList());
        }

        /// <summary>Pending queue (what sellers submitted for review).</summary>
        public IActionResult Pending()
        {
            var pending = _unitOfWork.Item
                .GetAll(filter: i => i.Status == ItemStatus.Pending, includeProperties: "Category,Seller")
                .OrderBy(i => i.DatePosted)
                .ToList();
            return View(pending);
        }

        public IActionResult Details(int? id)
        {
            if (id is null or 0) return NotFound();
            var item = _unitOfWork.Item
                .GetAll(filter: i => i.Id == id, includeProperties: "Category,Seller")
                .FirstOrDefault();
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var item = _unitOfWork.Item.Get(u => u.Id == id);
            if (item == null) return NotFound();

            item.Status = ItemStatus.Available;
            item.RejectionReason = null;
            _unitOfWork.Item.Update(item);
            _unitOfWork.Save();
            TempData["success"] = "Item approved.";
            return RedirectToAction(nameof(Pending));
        }

        public IActionResult Reject(int? id)
        {
            if (id is null or 0) return NotFound();
            var item = _unitOfWork.Item.Get(u => u.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, string? rejectionReason)
        {
            var item = _unitOfWork.Item.Get(u => u.Id == id);
            if (item == null) return NotFound();

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                ModelState.AddModelError(nameof(rejectionReason), "Please enter a rejection reason.");
                ViewBag.RejectionReason = rejectionReason;
                return View(item);
            }

            item.Status = ItemStatus.Rejected;
            item.RejectionReason = rejectionReason.Trim();
            _unitOfWork.Item.Update(item);
            _unitOfWork.Save();
            TempData["success"] = "Item rejected.";
            return RedirectToAction(nameof(Pending));
        }

        /// <summary>Admin edit for any item (bypasses the seller-ownership check).</summary>
        public IActionResult Edit(int? id)
        {
            if (id is null or 0) return NotFound();
            var item = _unitOfWork.Item
                .GetAll(filter: i => i.Id == id, includeProperties: "Category,Seller")
                .FirstOrDefault();
            if (item == null) return NotFound();

            LoadCategories(item.CategoryId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Quantity,Condition,Location,CategoryId,Status")] Item posted, IFormFile? imageFile)
        {
            var item = _unitOfWork.Item.Get(i => i.Id == id);
            if (item == null) return NotFound();

            ModelState.Remove(nameof(Item.Seller));
            ModelState.Remove(nameof(Item.Category));
            ModelState.Remove(nameof(Item.SellerId));
            ModelState.Remove(nameof(Item.Slug));

            if (imageFile is { Length: > 0 })
            {
                var err = ItemImageUpload.Validate(imageFile, required: false);
                if (err != null) ModelState.AddModelError("imageFile", err);
            }

            if (!ModelState.IsValid)
            {
                LoadCategories(posted.CategoryId);
                return View(item);
            }

            item.Name = posted.Name;
            item.Description = posted.Description;
            item.Price = posted.Price;
            item.Quantity = posted.Quantity;
            item.Condition = posted.Condition;
            item.Location = posted.Location;
            item.CategoryId = posted.CategoryId;
            item.Status = posted.Status;

            if (item.Status == ItemStatus.Available)
                item.RejectionReason = null;

            if (imageFile is { Length: > 0 })
                item.ImageUrl = await ItemImageUpload.SaveAsync(_env, imageFile, item.Slug);

            _unitOfWork.Item.Update(item);
            _unitOfWork.Save();
            TempData["success"] = "Item updated.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int? id)
        {
            if (id is null or 0) return NotFound();
            var item = _unitOfWork.Item
                .GetAll(filter: i => i.Id == id, includeProperties: "Category,Seller")
                .FirstOrDefault();
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _unitOfWork.Item.Get(i => i.Id == id);
            if (item == null) return NotFound();
            _unitOfWork.Item.Remove(item);
            _unitOfWork.Save();
            TempData["success"] = "Item deleted.";
            return RedirectToAction(nameof(Index));
        }

        private void LoadCategories(int? selected)
        {
            var list = _unitOfWork.Category.GetAll().OrderBy(c => c.Name).ToList();
            ViewBag.CategoryId = new SelectList(list, "Id", "Name", selected);
        }
    }
}
