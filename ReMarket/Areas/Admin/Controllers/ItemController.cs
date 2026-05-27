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

        public IActionResult Index()
        {
            return View();
        }

        #region API Calls

        [HttpGet]
        public IActionResult GetAll(ItemStatus? status)
        {
            var items = _unitOfWork.Item
                .GetAll(includeProperties: "Category,Seller")
                .AsEnumerable();

            if (status.HasValue)
                items = items.Where(i => i.Status == status.Value);

            var data = items
                .OrderByDescending(i => i.DatePosted)
                .Select(i => new
                {
                    i.Id,
                    i.Name,
                    i.Slug,
                    i.Price,
                    i.Quantity,
                    i.Status,
                    i.ImageUrl,
                    category = i.Category == null ? null : new { name = i.Category.Name },
                    seller = i.Seller == null ? null : new { email = i.Seller.Email }
                })
                .ToList();

            return Json(new { data });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var item = _unitOfWork.Item.Get(i => i.Id == id);
            if (item == null)
                return Json(new { success = false, message = "Error while deleting" });

            ItemImageUpload.DeleteAllGalleryFiles(_env, item);
            _unitOfWork.Item.Remove(item);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Item deleted successfully" });
        }

        #endregion

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
            item.RejectionReason = InputSanitizer.CleanText(model.RejectionReason);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Quantity,Condition,DeliveryOption,Location,CategoryId,Status,ImageUrl,MoreImageUrlsJson")] Item posted, IFormFile? coverImageFile, IFormFile[]? additionalImageFiles)
        {
            var item = _unitOfWork.Item.Get(i => i.Id == id);
            if (item == null) return NotFound();

            ModelState.Remove(nameof(Item.Seller));
            ModelState.Remove(nameof(Item.Category));
            ModelState.Remove(nameof(Item.SellerId));
            ModelState.Remove(nameof(Item.Slug));

            if (coverImageFile is { Length: > 0 })
            {
                var coverErr = ItemImageUpload.Validate(coverImageFile, required: false);
                if (coverErr != null)
                    ModelState.AddModelError("coverImageFile", coverErr);
            }

            var newFiles = additionalImageFiles?.Where(f => f is { Length: > 0 }).ToList() ?? new List<IFormFile>();
            if (newFiles.Count > 0)
            {
                var currentCount = ItemGallery.GetAllImageUrls(item).Count;
                if (currentCount >= ItemGallery.MaxImages)
                    ModelState.AddModelError("additionalImageFiles", "This item already has the maximum of 8 images.");
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
                item.Name = posted.Name;
                item.Description = posted.Description;
                item.Price = posted.Price;
                item.Quantity = posted.Quantity;
                item.Condition = posted.Condition;
                item.DeliveryOption = posted.DeliveryOption;
                item.Location = posted.Location;
                item.CategoryId = posted.CategoryId;
                item.Status = ItemStatus.Pending;
                LoadCategories(item.CategoryId);
                return View(item);
            }

            InputSanitizer.SanitizeItem(posted);
            item.Name = posted.Name;
            item.Description = posted.Description;
            item.Price = posted.Price;
            item.Quantity = posted.Quantity;
            item.Condition = posted.Condition;
            item.DeliveryOption = posted.DeliveryOption;
            item.Location = posted.Location;
            item.CategoryId = posted.CategoryId;
            item.Status = ItemStatus.Pending;

            if (item.Status == ItemStatus.Available)
                item.RejectionReason = null;

            if (coverImageFile is { Length: > 0 })
                await ItemImageUpload.ReplaceCoverAsync(_env, item, coverImageFile);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteImage(int id, int imageIndex, string? returnTo)
        {
            var item = _unitOfWork.Item.Get(i => i.Id == id);
            if (item == null) return NotFound();

            if (!ItemImageUpload.TryRemoveImageAt(_env, item, imageIndex, out var error))
                TempData["error"] = error;
            else
            {
                _unitOfWork.Item.Update(item);
                _unitOfWork.Save();
                TempData["success"] = "Image removed.";
            }

            return returnTo == "edit"
                ? RedirectToAction(nameof(Edit), new { id })
                : RedirectToAction(nameof(Index));
        }

        private void LoadCategories(int? selected)
        {
            var list = _unitOfWork.Category.GetAll().OrderBy(c => c.Name).ToList();
            ViewBag.CategoryId = new SelectList(list, "Id", "Name", selected);
        }
    }
}
