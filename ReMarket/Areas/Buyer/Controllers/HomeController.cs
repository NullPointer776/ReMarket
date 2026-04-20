using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;

namespace ReMarket.Web.Areas.Buyer.Controllers
{
    [Area("Buyer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            ViewBag.Categories = _unitOfWork.Category
                .GetAll(filter: c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.LatestItems = _unitOfWork.Item
                .GetAll(filter: i => i.Status == ItemStatus.Available, includeProperties: "Category")
                .OrderByDescending(i => i.DatePosted)
                .Take(8)
                .ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
