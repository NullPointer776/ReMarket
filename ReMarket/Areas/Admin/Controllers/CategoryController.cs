using Microsoft.AspNetCore.Mvc;

namespace ReMarket.Web.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
