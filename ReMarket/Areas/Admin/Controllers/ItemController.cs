using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;
using ReMarket.Models.ViewModel;
using ReMarket.Utility;

namespace ReMarket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //Image upload size limit
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

            return View(items.OrderByDescending(i => i.DatePosted).ToList());
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
            return RedirectToAction(nameof(Index));
        }

        // Shows reject form (reason required when posted).
        public IActionResult Reject(int? id)
        {
            if (id is null or 0) return NotFound();
            var item = _unitOfWork.Item.Get(u => u.Id == id);
            if (item == null) return NotFound();
            return View(new RejectItemViewModel
            {
                Id = item.Id,
                ItemName = item.Name,
                RejectionReason = null
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(RejectItemViewModel model)
        {
            var item = _unitOfWork.Item.Get(u => u.Id == model.Id);
            if (item == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.ItemName = item.Name;
                return View(model);
            }

            item.Status = ItemStatus.Rejected;
            item.RejectionReason = model.RejectionReason!.Trim();
            _unitOfWork.Item.Update(item);
            _unitOfWork.Save();
            TempData["success"] = "Item rejected.";
            return RedirectToAction(nameof(Index));
        }
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Quantity,Condition,DeliveryOption,Location,CategoryId,Status")] Item posted, IFormFile[]? additionalImageFiles)
        {
            var item = _unitOfWork.Item.Get(i => i.Id == id);
            if (item == null) return NotFound();

            ModelState.Remove(nameof(Item.Seller));
            ModelState.Remove(nameof(Item.Category));
            ModelState.Remove(nameof(Item.SellerId));
            ModelState.Remove(nameof(Item.Slug));

            var newFiles = additionalImageFiles?.Where(f => f is { Length: > 0 }).ToList() ?? new List<IFormFile>();
            if (newFiles.Count > 0)
            {
                var currentCount = ItemGallery.GetAllImageUrls(item).Count;
                if (currentCount >= ItemGallery.MaxImages)
                {
                    ModelState.AddModelError("additionalImageFiles", "This item already has the maximum of 8 images.");
                }
                else
                {
                    var room = ItemGallery.MaxImages - currentCount;
                    if (newFiles.Count > room)
                        ModelState.AddModelError("additionalImageFiles", $"You can add at most {room} more image(s).");
                    else
                    {
                        var err = ItemImageUpload.ValidateImageFiles(additionalImageFiles, requireAtLeastOne: false);
                        if (err != null) ModelState.AddModelError("additionalImageFiles", err);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                // Copy posted fields back so the user sees what they submitted.
                item.Name = posted.Name;
                item.Description = posted.Description;
                item.Price = posted.Price;
                item.Quantity = posted.Quantity;
                item.Condition = posted.Condition;
                item.DeliveryOption = posted.DeliveryOption;
                item.Location = posted.Location;
                item.CategoryId = posted.CategoryId;
                item.Status = posted.Status;
                LoadCategories(item.CategoryId);
                return View(item);
            }

            item.Name = posted.Name;
            item.Description = posted.Description;
            item.Price = posted.Price;
            item.Quantity = posted.Quantity;
            item.Condition = posted.Condition;
            item.DeliveryOption = posted.DeliveryOption;
            item.Location = posted.Location;
            item.CategoryId = posted.CategoryId;
            item.Status = posted.Status;

            if (item.Status == ItemStatus.Available)
                item.RejectionReason = null;

            if (newFiles.Count > 0)
            {
                var urls = ItemGallery.GetAllImageUrls(item).ToList();
                foreach (var file in newFiles)
                {
                    if (urls.Count >= ItemGallery.MaxImages) break;
                    var url = await ItemImageUpload.SaveAsync(_env, file, item.Slug!, urls.Count);
                    urls.Add(url);
                }

                ItemGallery.SetGalleryFromUrls(item, urls);
            }

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
        //Get all categories 
        private void LoadCategories(int? selected)
        {
            var list = _unitOfWork.Category.GetAll().OrderBy(c => c.Name).ToList();
            ViewBag.CategoryId = new SelectList(list, "Id", "Name", selected);
        }
    }
}
