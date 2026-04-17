using Microsoft.AspNetCore.Mvc;

namespace ReMarket.Web.Areas.Seller.Controllers
{
    public class ItemController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
