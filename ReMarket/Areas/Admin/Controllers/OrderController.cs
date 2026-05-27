using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.DataAccess.Services;
using ReMarket.Models;
using ReMarket.Models.ViewModel;
using ReMarket.Utility;

namespace ReMarket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM OrderVM { get; set; } = new();

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API Calls

        [HttpGet]
        public IActionResult GetAll(string? status)
        {
            // Admin can see all orders
            IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader
                .GetAll(includeProperties: "ApplicationUser")
                .ToList();

            if (!string.IsNullOrEmpty(status))
            {
                orderHeaders = status.ToLower() switch
                {
                    "pending" => orderHeaders.Where(u => u.OrderStatus == SD.StatusPending),
                    "approved" => orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved),
                    "inprocess" => orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess),
                    "completed" => orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped),
                    _ => orderHeaders
                };
            }

            return Json(new { data = orderHeaders });
        }

        #endregion

        public IActionResult Details(int orderId)
        {
            OrderVM = new OrderVM
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(
                    u => u.Id == orderId,
                    includeProperties: "ApplicationUser")!,
                OrderDetail = _unitOfWork.OrderDetail
                    .GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Item")
                    .ToList()
            };

            if (OrderVM.OrderHeader == null)
                return NotFound();

            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult UpdateOrderDetail()
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please correct the order details and try again.";
                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
            }

            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(
                u => u.Id == OrderVM.OrderHeader.Id,
                includeProperties: null,
                tracked: true);

            if (orderHeaderFromDb == null)
                return NotFound();

            InputSanitizer.SanitizeOrderHeader(OrderVM.OrderHeader);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;

            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Order details updated successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order status updated to Processing.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(
                u => u.Id == OrderVM.OrderHeader.Id,
                tracked: true);

            if (orderHeader == null)
                return NotFound();

            OrderVM.OrderHeader.TrackingNumber = InputSanitizer.CleanText(OrderVM.OrderHeader.TrackingNumber);
            OrderVM.OrderHeader.Carrier = InputSanitizer.CleanText(OrderVM.OrderHeader.Carrier);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["success"] = "Order shipped successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);

            if (orderHeader == null)
                return NotFound();

            if (!OrderCancellationService.CanCancel(orderHeader))
            {
                TempData["error"] = "This order can no longer be cancelled.";
                return RedirectToAction(nameof(Details), new { orderId = orderHeader.Id });
            }

            try
            {
                OrderCancellationService.Cancel(_unitOfWork, orderHeader);
                TempData["success"] = "Order cancelled successfully.";
            }
            catch (Stripe.StripeException)
            {
                TempData["error"] = "Payment refund failed. Please try again or contact support.";
            }

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
    }
}