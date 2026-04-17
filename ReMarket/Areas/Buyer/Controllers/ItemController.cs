using Microsoft.AspNetCore.Mvc;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;

namespace ReMarket.Web.Areas.Buyer.Controllers
{
    [Area("Buyer")]
    public class ItemController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly IUnitOfWork _unitOfWork;
        public ItemController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index(int id)
        {
            List<Item> allItems = _unitOfWork.Item.GetAll().ToList();
            List<Item> approvedItems = allItems.Where(c => c.Status == ItemStatus.Available).ToList();
            return View(approvedItems);
        }
        public IActionResult Detail(int id)
        {
            Item? itemFromDb = _unitOfWork.Item.Get(u => u.Id == id);
            if (itemFromDb == null)
            {
                return NotFound();
            }
            return View(itemFromDb);
        }
        public IActionResult Buy(int id)
        {
            Item? itemFromDb = _unitOfWork.Item.Get(u => u.Id == id);
            if (itemFromDb == null)
            {
                return NotFound();
            }
            return View(itemFromDb);  
        }
        [HttpPost]
        public IActionResult BuyItem(int id)
        {
            Item? itemFromDb = _unitOfWork.Item.Get(u => u.Id == id);
            if (itemFromDb == null)
            {
                return NotFound();
            }
            itemFromDb.Quantity -= 1;
            if (itemFromDb.Quantity <= 0){
                itemFromDb.Status = ItemStatus.SoldOut;
            }
            _unitOfWork.Item.Update(itemFromDb);
            _unitOfWork.Save();
            return RedirectToAction("Index", "Home", new { area = "Buyer" });
        }
    }
}
