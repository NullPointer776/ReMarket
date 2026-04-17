using Microsoft.AspNetCore.Mvc;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;

namespace ReMarket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ItemController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ItemController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Item> allItems = _unitOfWork.Item.GetAll().ToList();
            List<Item> pendingItems = allItems.Where(c => c.Status == ItemStatus.Pending).ToList();
            return View(pendingItems);
        }
        public IActionResult Approve(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Item? itemFromDb = _unitOfWork.Item.Get(u => u.Id == id);
            if (itemFromDb == null)
            {
                return NotFound();
            }
            itemFromDb.Status = ItemStatus.Available;
            _unitOfWork.Item.Update(itemFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Item approved successfully";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Approve(int id, Item obj)
        {
            if (id != obj.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Item.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Item approved successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        public IActionResult Details(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Item? itemFromDb = _unitOfWork.Item.Get(u => u.Id == id);
            if (itemFromDb == null)
            {
                return NotFound();
            }
            return View(itemFromDb);
        }
        public IActionResult Reject(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Item? itemFromDb = _unitOfWork.Item.Get(u => u.Id == id);
            if (itemFromDb == null)
            {
                return NotFound();
            }
            return View(itemFromDb);
        }
        [HttpPost]
        public IActionResult Reject(int id, Item obj)
        {
            if (id != obj.Id)
            {
                return NotFound();
            }
            Item? itemFromDb = _unitOfWork.Item.Get(u => u.Id == id);
            if (itemFromDb == null)
            {
                return NotFound();
            }
            itemFromDb.Status = ItemStatus.Rejected;
            _unitOfWork.Item.Update(itemFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Item rejected successfully";
            return RedirectToAction("Index");
        }
    } 
}