using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;

namespace ReMarket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ItemController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ItemController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var pending = _unitOfWork.Item
                .GetAll(filter: i => i.Status == ItemStatus.Pending, includeProperties: "Category,Seller")
                .OrderBy(i => i.DatePosted)
                .ToList();
            return View(pending);
        }

        public IActionResult Details(int? id)
        {
            if (id is null or 0)
                return NotFound();

            var item = _unitOfWork.Item
                .GetAll(filter: i => i.Id == id, includeProperties: "Category,Seller")
                .FirstOrDefault();
            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var item = _unitOfWork.Item.Get(u => u.Id == id);
            if (item == null)
                return NotFound();

            item.Status = ItemStatus.Available;
            item.RejectionReason = null;
            _unitOfWork.Item.Update(item);
            _unitOfWork.Save();
            TempData["success"] = "已通过审核。";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Reject(int? id)
        {
            if (id is null or 0)
                return NotFound();

            var item = _unitOfWork.Item.Get(u => u.Id == id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, string? rejectionReason)
        {
            var item = _unitOfWork.Item.Get(u => u.Id == id);
            if (item == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                ModelState.AddModelError(nameof(rejectionReason), "请填写拒绝原因。");
                ViewBag.RejectionReason = rejectionReason;
                return View(item);
            }

            item.Status = ItemStatus.Rejected;
            item.RejectionReason = rejectionReason.Trim();
            _unitOfWork.Item.Update(item);
            _unitOfWork.Save();
            TempData["success"] = "已拒绝该商品。";
            return RedirectToAction(nameof(Index));
        }
    }
}
