using Microsoft.AspNetCore.Mvc;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;

namespace ReMarket.Web.Areas.Buyer.Controllers
{
    [Area("Buyer")]
    public class ItemController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ItemController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string? search, int? categoryId)
        {
            var items = _unitOfWork.Item
                .GetAll(filter: i => i.Status == ItemStatus.Available, includeProperties: "Category")
                .AsEnumerable();

            if (categoryId.HasValue)
                items = items.Where(i => i.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                items = items.Where(i =>
                    i.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (i.Description != null && i.Description.Contains(q, StringComparison.OrdinalIgnoreCase)));
            }

            var list = items.OrderByDescending(i => i.DatePosted).ToList();

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryList = _unitOfWork.Category.GetAll().OrderBy(c => c.Name).ToList();

            return View(list);
        }

        public IActionResult Detail(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var item = _unitOfWork.Item
                .GetAll(
                    filter: i => i.Slug == slug && i.Status == ItemStatus.Available,
                    includeProperties: "Category,Seller")
                .FirstOrDefault();
            if (item == null)
                return NotFound();

            return View(item);
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
                TempData["error"] = "该商品已售罄。";
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
                TempData["error"] = "该商品已售罄。";
                return RedirectToAction(nameof(Detail), new { slug });
            }

            item.Quantity -= 1;
            if (item.Quantity <= 0)
                item.Status = ItemStatus.SoldOut;

            _unitOfWork.Item.Update(item);
            _unitOfWork.Save();
            TempData["success"] = "购买成功（作业版：未接入支付）。";
            return RedirectToAction(nameof(Index));
        }
    }
}
