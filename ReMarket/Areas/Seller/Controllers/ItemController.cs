using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QRCoder;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;
using ReMarket.Utility;

namespace ReMarket.Web.Areas.Seller.Controllers
{
    [Area("Seller")]
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

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public IActionResult Preview(int? id)
        {
            var uid = UserId;
            if (uid == null)
                return Challenge();

            var item = _unitOfWork.Item
                .GetAll(filter: i => i.Id == id && i.SellerId == uid, includeProperties: "Category")
                .FirstOrDefault();
            if (item == null)
                return NotFound();
            return View(item);
        }

        public IActionResult Index()
        {
            var uid = UserId;
            if (uid == null)
                return Challenge();

            var items = _unitOfWork.Item
                .GetAll(filter: i => i.SellerId == uid, includeProperties: "Category")
                .OrderByDescending(i => i.DatePosted)
                .ToList();
            return View(items);
        }

        public IActionResult Create()
        {
            LoadCategories();
            return View(new Item { Quantity = 1, Condition = Condition.Good });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item model, IFormFile? imageFile)
        {
            var uid = UserId;
            if (uid == null)
                return Challenge();

            model.SellerId = uid;
            model.Status = ItemStatus.Pending;
            model.DatePosted = DateTime.UtcNow;
            model.Slug = NextAvailableSlug(SlugHelper.ToSlug(model.Name));

            ClearNavigationModelState();

            var imageError = ItemImageUpload.Validate(imageFile, required: true);
            if (imageError != null)
                ModelState.AddModelError("imageFile", imageError);

            if (ModelState.IsValid)
            {
                model.ImageUrl = await ItemImageUpload.SaveAsync(_env, imageFile!, model.Slug);
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
            var uid = UserId;
            if (uid == null)
                return Challenge();

            var item = GetOwnedItem(id, uid);
            if (item == null)
                return NotFound();

            LoadCategories();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Description,Price,Quantity,Condition,Location,CategoryId")] Item model, IFormFile? imageFile)
        {
            var uid = UserId;
            if (uid == null)
                return Challenge();

            var existing = GetOwnedItem(id, uid);
            if (existing == null)
                return NotFound();

            ClearNavigationModelState();

            if (imageFile is { Length: > 0 })
            {
                var imageError = ItemImageUpload.Validate(imageFile, required: false);
                if (imageError != null)
                    ModelState.AddModelError("imageFile", imageError);
            }

            if (ModelState.IsValid)
            {
                existing.Name = model.Name;
                existing.Description = model.Description;
                existing.Price = model.Price;
                existing.Quantity = model.Quantity;
                existing.Condition = model.Condition;
                existing.Location = model.Location;
                existing.CategoryId = model.CategoryId;
                existing.Status = ItemStatus.Pending;
                existing.RejectionReason = null;

                if (imageFile is { Length: > 0 })
                    existing.ImageUrl = await ItemImageUpload.SaveAsync(_env, imageFile, existing.Slug);

                await WriteQrPngAsync(existing);
                _unitOfWork.Item.Update(existing);
                _unitOfWork.Save();

                TempData["success"] = "Item updated and is pending admin review.";
                return RedirectToAction(nameof(Index));
            }

            LoadCategories();
            model.Id = existing.Id;
            model.Slug = existing.Slug;
            model.SellerId = existing.SellerId;
            model.Status = existing.Status;
            model.DatePosted = existing.DatePosted;
            model.ImageUrl = existing.ImageUrl;
            model.QrCodeUrl = existing.QrCodeUrl;
            return View(model);
        }

        public IActionResult Delete(int? id)
        {
            var uid = UserId;
            if (uid == null)
                return Challenge();

            var item = GetOwnedItem(id, uid);
            if (item == null)
                return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var uid = UserId;
            if (uid == null)
                return Challenge();

            var item = GetOwnedItem(id, uid);
            if (item == null)
                return NotFound();

            _unitOfWork.Item.Remove(item);
            _unitOfWork.Save();
            TempData["success"] = "Item deleted.";
            return RedirectToAction(nameof(Index));
        }

        private Item? GetOwnedItem(int? id, string uid)
        {
            if (id is null or 0)
                return null;
            var item = _unitOfWork.Item.Get(i => i.Id == id);
            if (item == null || item.SellerId != uid)
                return null;
            return item;
        }

        private void LoadCategories()
        {
            var list = _unitOfWork.Category.GetAll().OrderBy(c => c.Name).ToList();
            ViewBag.CategoryId = new SelectList(list, "Id", "Name");
        }

        private void ClearNavigationModelState()
        {
            ModelState.Remove(nameof(Item.Seller));
            ModelState.Remove(nameof(Item.Category));
            ModelState.Remove(nameof(Item.SellerId));
            ModelState.Remove(nameof(Item.Slug));
        }

        private string NextAvailableSlug(string baseSlug)
        {
            var slug = baseSlug;
            var n = 1;
            while (_unitOfWork.Item.Get(i => i.Slug == slug) != null)
            {
                slug = $"{baseSlug}-{n++}";
            }
            return slug;
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
