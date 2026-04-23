using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;
using ReMarket.Models.ViewModel;
using ReMarket.Utility;

namespace ReMarket.Web.Areas.Buyer.Controllers
{
    [Area("Buyer")]
    [Authorize(Roles = "Buyer, Admin, Anonymous")]
    public class ItemController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ItemController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string? search, int? categoryId, string? condition, string? location, string? deliveryOption, string? sortBy, string? activeTab)
        {
            var items = _unitOfWork.Item
                .GetAll(filter: i => i.Status == ItemStatus.Available, includeProperties: "Category,Seller")
                .AsEnumerable();

            // Filtering by category
            if (categoryId.HasValue)
            {
                items = items.Where(i => i.CategoryId == categoryId.Value);
            }

            // search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                items = items.Where(i =>
                    i.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (i.Description != null && i.Description.Contains(q, StringComparison.OrdinalIgnoreCase)));
            }

            // Condition filter
            if (!string.IsNullOrWhiteSpace(condition) && Enum.TryParse<Condition>(condition, out var conditionEnum))
            {
                items = items.Where(i => i.Condition == conditionEnum);
            }

            // Location filter
            if (!string.IsNullOrWhiteSpace(location))
            {
                items = items.Where(i => i.Location != null && i.Location.Contains(location, StringComparison.OrdinalIgnoreCase));
            }

            // Delivery Option Filter
            if (!string.IsNullOrWhiteSpace(deliveryOption) && Enum.TryParse<DeliveryOption>(deliveryOption, out var deliveryEnum))
            {
                items = items.Where(i => i.DeliveryOption == deliveryEnum);
            }

            // Sorting
            items = sortBy switch
            {
                "oldest" => items.OrderBy(i => i.DatePosted),
                "price_high" => items.OrderByDescending(i => i.Price),
                "price_low" => items.OrderBy(i => i.Price),
                _ => items.OrderByDescending(i => i.DatePosted)  // Default newest
            };

            var viewModel = new ItemIndexViewModel
            {
                Items = items.ToList(),
                Categories = _unitOfWork.Category
                    .GetAll(filter: c => c.IsActive, includeProperties: "SubCategories")
                    .OrderBy(c => c.Name)
                    .ToList(),
                Search = search,
                CategoryId = categoryId,
                Condition = condition,
                Location = location,
                DeliveryOption = deliveryOption,
                SortBy = sortBy,
                ActiveTab = activeTab ?? "category"
            };

            return View(viewModel);
        }

        public IActionResult Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            var item = _unitOfWork.Item.Get(
                filter: i => i.Slug == slug && i.Status == ItemStatus.Available,
                includeProperties: "Category,Seller"
            );

            if (item == null)
                return NotFound();

            var imageUrls = ItemGallery.GetAllImageUrls(item);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwner = userId != null && userId == item.SellerId;
            var canAddMore = isOwner && imageUrls.Count < ItemGallery.MaxImages;

            var viewModel = new ItemDetailViewModel
            {
                Item = item,
                SellerName = item.Seller?.UserName ?? "Unknown",
                SellerEmail = item.Seller?.Email ?? "Not provided",
                ImageUrls = imageUrls,
                IsListingOwner = isOwner,
                CanAddMoreImages = canAddMore,
                AddMoreImagesUrl = canAddMore
                    ? Url.Action("Edit", "Item", new { area = "Seller", id = item.Id }) + "#add-images"
                    : null
            };

            return View(viewModel);
        }

        public IActionResult Buy(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var item = _unitOfWork.Item.Get(i => i.Slug == slug && i.Status == ItemStatus.Available);
            if (item == null)
                return NotFound();
            if (item.Quantity <= 0)
            {
                TempData["error"] = "This item is sold out.";
                return RedirectToAction(nameof(Detail), new { slug });
            }

            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Purchase(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var item = _unitOfWork.Item.Get(i => i.Slug == slug && i.Status == ItemStatus.Available);
            if (item == null)
                return NotFound();
            if (item.Quantity <= 0)
            {
                TempData["error"] = "This item is sold out.";
                return RedirectToAction(nameof(Detail), new { slug });
            }

            item.Quantity -= 1;
            if (item.Quantity <= 0)
                item.Status = ItemStatus.SoldOut;

            _unitOfWork.Item.Update(item);
            _unitOfWork.Save();
            TempData["success"] = "Purchase recorded (assignment build: payment not integrated).";
            return RedirectToAction(nameof(Index));
        }
    }
}
