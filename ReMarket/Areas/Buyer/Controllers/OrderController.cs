using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.DataAccess.Services;
using ReMarket.Models;
using ReMarket.Models.ViewModel;
using ReMarket.Utility;
using Stripe;

namespace ReMarket.Web.Areas.Buyer.Controllers
{
    [Area("Buyer")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var orders = _unitOfWork.OrderHeader
                .GetAll(o => o.ApplicationUserId == UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        public IActionResult Details(int orderId)
        {
            var orderHeader = GetOwnedOrder(orderId);
            if (orderHeader == null)
                return NotFound();

            var vm = new OrderVM
            {
                OrderHeader = orderHeader,
                OrderDetail = _unitOfWork.OrderDetail
                    .GetAll(d => d.OrderHeaderId == orderId, includeProperties: "Item")
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var orderHeader = GetOwnedOrder(id);
            if (orderHeader == null)
                return NotFound();

            if (!OrderCancellationService.CanCancel(orderHeader))
            {
                TempData["error"] = "This order can no longer be cancelled.";
                return RedirectToAction(nameof(Details), new { orderId = id });
            }

            try
            {
                OrderCancellationService.Cancel(_unitOfWork, orderHeader);
                TempData["success"] = "Order cancelled successfully.";
            }
            catch (StripeException)
            {
                TempData["error"] = "Payment refund failed. Please contact support.";
            }

            return RedirectToAction(nameof(Details), new { orderId = id });
        }

        private OrderHeader? GetOwnedOrder(int orderId) =>
            _unitOfWork.OrderHeader.Get(
                o => o.Id == orderId && o.ApplicationUserId == UserId,
                includeProperties: "ApplicationUser");
    }
}
