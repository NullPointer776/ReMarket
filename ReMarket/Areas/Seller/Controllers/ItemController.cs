using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QRCoder;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;
using ReMarket.Models.ViewModel;
using ReMarket.Utility;
using System.Security.Claims;

namespace ReMarket.Web.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize]
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public class ItemController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;
        private string? _cachedUserId;
        private string? UserId => _cachedUserId ??= User.FindFirstValue(ClaimTypes.NameIdentifier);

        public ItemController(IUnitOfWork unitOfWork, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _env = env;
        }

        public IActionResult Index()
        {
            var items = _unitOfWork.Item
                .GetAll(filter: i => i.SellerId == UserId, includeProperties: "Category")
                .OrderByDescending(i => i.DatePosted)
                .ToList();
            return View(items);
        }

        public IActionResult Preview(int? id)
        {
            var item = _unitOfWork.Item.Get(i => i.Id == id && i.SellerId == UserId, includeProperties: "Category");
            return item == null ? NotFound() : View(item);
        }

        public IActionResult Create()
        {
            LoadCategories();
            return View(new Item { Quantity = 1, Condition = Condition.Good });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item model, IFormFile[]? imageFiles)
        {
            model.SellerId = UserId!;
            model.Status = ItemStatus.Pending;
            model.DatePosted = DateTime.UtcNow;
            model.Slug = NextAvailableSlug(SlugHelper.ToSlug(model.Name));

            ClearNavigationModelState();

            var imageError = ItemImageUpload.ValidateImageFiles(imageFiles, requireAtLeastOne: true);
            if (imageError != null)
                ModelState.AddModelError("imageFiles", imageError);

            if (ModelState.IsValid)
            {
                var fileList = imageFiles!.Where(f => f is { Length: > 0 }).ToList();
                var urls = new List<string>(ItemGallery.MaxImages);
                for (var i = 0; i < fileList.Count; i++)
                {
                    var url = await ItemImageUpload.SaveAsync(_env, fileList[i], model.Slug!, i);
                    urls.Add(url);
                }

                ItemGallery.SetGalleryFromUrls(model, urls);
                _unitOfWork.Item.Add(model);
                _unitOfWork.Save();

                await WriteQrPngAsync(model);
                _unitOfWork.Item.Update(model);
                _unitOfWork.Save();

                TempData["success"] = "Item submitted and is pending admin review.";
                return RedirectToAction(nameof(Index));
            }

            LoadCategories();
            return View(model);
        }

        public IActionResult Edit(int? id)
        {
            var item = GetOwnedItem(id);
            if (item == null) return NotFound();

            LoadCategories(item.CategoryId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Description,Price,Quantity,Condition,DeliveryOption,Location,CategoryId")] Item model, IFormFile[]? additionalImageFiles)
        {
            var existing = GetOwnedItem(id);
            if (existing == null) return NotFound();

            ClearNavigationModelState();

            var newFiles = additionalImageFiles?.Where(f => f is { Length: > 0 }).ToList() ?? new List<IFormFile>();
            if (newFiles.Count > 0)
            {
                var currentCount = ItemGallery.GetAllImageUrls(existing).Count;
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

            if (ModelState.IsValid)
            {
                existing.Name = model.Name;
                existing.Description = model.Description;
                existing.Price = model.Price;
                existing.Quantity = model.Quantity;
                existing.Condition = model.Condition;
                existing.DeliveryOption = model.DeliveryOption;
                existing.Location = model.Location;
                existing.CategoryId = model.CategoryId;
                existing.Status = ItemStatus.Pending;
                existing.RejectionReason = null;

                if (newFiles.Count > 0)
                {
                    var urls = ItemGallery.GetAllImageUrls(existing).ToList();
                    foreach (var file in newFiles)
                    {
                        if (urls.Count >= ItemGallery.MaxImages) break;
                        var url = await ItemImageUpload.SaveAsync(_env, file, existing.Slug!, urls.Count);
                        urls.Add(url);
                    }
                    ItemGallery.SetGalleryFromUrls(existing, urls);
                }

                await WriteQrPngAsync(existing);
                _unitOfWork.Item.Update(existing);
                _unitOfWork.Save();

                TempData["success"] = "Item updated and is pending admin review.";
                return RedirectToAction(nameof(Index));
            }

            LoadCategories(model.CategoryId);
            model.Id = existing.Id;
            model.Slug = existing.Slug;
            model.SellerId = existing.SellerId;
            model.Status = existing.Status;
            model.DatePosted = existing.DatePosted;
            model.ImageUrl = existing.ImageUrl;
            model.MoreImageUrlsJson = existing.MoreImageUrlsJson;
            model.QrCodeUrl = existing.QrCodeUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteImage(int id, int imageIndex, string? returnSlug)
        {
            var item = GetOwnedItem(id);
            if (item == null) return NotFound();

            var urls = ItemGallery.GetAllImageUrls(item).ToList();
            if (imageIndex < 0 || imageIndex >= urls.Count) return NotFound();
            if (urls.Count <= 1)
            {
                TempData["error"] = "Add another image before removing the only one.";
                var fallbackSlug = !string.IsNullOrWhiteSpace(returnSlug) ? returnSlug : item.Slug;
                return string.IsNullOrEmpty(fallbackSlug)
                    ? RedirectToAction(nameof(Index))
                    : RedirectToAction("Detail", "Item", new { area = "Buyer", slug = fallbackSlug });
            }

            var removed = urls[imageIndex];
            urls.RemoveAt(imageIndex);
            ItemImageUpload.TryDeleteItemImageFile(_env, removed);
            ItemGallery.SetGalleryFromUrls(item, urls);
            _unitOfWork.Item.Update(item);
            _unitOfWork.Save();

            TempData["success"] = "Image removed.";
            var slug = !string.IsNullOrWhiteSpace(returnSlug) ? returnSlug : item.Slug;
            return string.IsNullOrEmpty(slug)
                ? RedirectToAction(nameof(Index))
                : RedirectToAction("Detail", "Item", new { area = "Buyer", slug });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var item = GetOwnedItem(id);
            if (item == null) return NotFound();

            _unitOfWork.Item.Remove(item);
            _unitOfWork.Save();
            TempData["success"] = "Item deleted.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult RejectReason(int id)
        {
            var item = _unitOfWork.Item.Get(i => i.Id == id && i.SellerId == UserId);
            if (item == null || item.Status != ItemStatus.Rejected) return NotFound();

            return View(new RejectItemViewModel
            {
                Id = item.Id,
                ItemName = item.Name,
                RejectionReason = item.RejectionReason
            });
        }

        private Item? GetOwnedItem(int? id)
        {
            if (id is null or 0) return null;
            var item = _unitOfWork.Item.Get(i => i.Id == id);
            return item != null && item.SellerId == UserId ? item : null;
        }

        private void LoadCategories(int? selectedId = null)
        {
            var list = _unitOfWork.Category.GetAll().OrderBy(c => c.Name).ToList();
            ViewBag.CategoryId = new SelectList(list, "Id", "Name", selectedId);
        }

        private void ClearNavigationModelState()
        {
            ModelState.Remove(nameof(Item.SellerId));
            ModelState.Remove(nameof(Item.Slug));
        }

        private string NextAvailableSlug(string baseSlug)
        {
            var slug = baseSlug;
            var existingSlugs = _unitOfWork.Item.GetAll(i => i.Slug.StartsWith(baseSlug))
                .Select(i => i.Slug)
                .ToHashSet();

            if (!existingSlugs.Contains(slug)) return slug;

            var n = 1;
            while (existingSlugs.Contains($"{baseSlug}-{n}")) n++;
            return $"{baseSlug}-{n}";
        }

        private async Task WriteQrPngAsync(Item item)
        {
            var url = $"{Request.Scheme}://{Request.Host}/item/{item.Slug}";
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var pngQr = new PngByteQRCode(qrData);
            var bytes = pngQr.GetGraphic(20);
            var dir = Path.Combine(_env.WebRootPath, "images", "qrcodes");
            Directory.CreateDirectory(dir);
            var fileName = $"{item.Slug}.png";
            await System.IO.File.WriteAllBytesAsync(Path.Combine(dir, fileName), bytes);
            item.QrCodeUrl = "/images/qrcodes/" + fileName;
        }
    }
}