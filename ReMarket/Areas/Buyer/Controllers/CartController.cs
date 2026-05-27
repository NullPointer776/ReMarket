using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;
using ReMarket.Models.ViewModel;
using ReMarket.Utility;
using Stripe;
using Stripe.Checkout;

namespace ReMarket.Web.Areas.Buyer.Controllers
{
    [Area("Buyer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; } = new();

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var userId = GetUserId();
            ShoppingCartVM = BuildCartViewModel(userId);
            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var userId = GetUserId();
            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.Id == cartId && u.ApplicationUserId == userId,
                includeProperties: "Item",
                tracked: true);

            if (cartFromDb?.Item == null)
                return NotFound();

            if (cartFromDb.Count >= cartFromDb.Item.Quantity)
            {
                TempData["error"] = $"Only {cartFromDb.Item.Quantity} available in stock.";
                return RedirectToAction(nameof(Index));
            }

            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var userId = GetUserId();
            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.Id == cartId && u.ApplicationUserId == userId,
                tracked: true);

            if (cartFromDb == null)
                return NotFound();

            if (cartFromDb.Count <= 1)
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var userId = GetUserId();
            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.Id == cartId && u.ApplicationUserId == userId,
                tracked: true);

            if (cartFromDb == null)
                return NotFound();

            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            ShoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartList = _unitOfWork.ShoppingCart
                    .GetAll(u => u.ApplicationUserId == userId, includeProperties: "Item")
                    .ToList(),
                OrderHeader = new OrderHeader()
            };

            if (!ShoppingCartVM.ShoppingCartList.Any())
            {
                TempData["error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(
                u => u.Id == userId)!;

            var user = ShoppingCartVM.OrderHeader.ApplicationUser;
            var fullName = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = user.UserName ?? user.Email ?? string.Empty;

            ShoppingCartVM.OrderHeader.Name = fullName;
            ShoppingCartVM.OrderHeader.PhoneNumber = user.PhoneNumber ?? string.Empty;
            ShoppingCartVM.OrderHeader.StreetAddress = user.StreetAddress ?? string.Empty;
            ShoppingCartVM.OrderHeader.City = user.City ?? string.Empty;
            ShoppingCartVM.OrderHeader.State = user.State ?? user.Suburb ?? string.Empty;
            ShoppingCartVM.OrderHeader.PostalCode = user.PostalCode ?? string.Empty;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price;

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == userId, includeProperties: "Item")
                .ToList();

            if (!ShoppingCartVM.ShoppingCartList.Any())
            {
                TempData["error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            ShoppingCartVM.OrderHeader.OrderTotal = 0;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price;

            ModelState.Remove("OrderHeader.ApplicationUser");
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("ShoppingCartList", StringComparison.Ordinal)).ToList())
                ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(
                    u => u.Id == userId)!;
                return View(nameof(Summary), ShoppingCartVM);
            }

            InputSanitizer.SanitizeOrderHeader(ShoppingCartVM.OrderHeader);

            // Always use immediate payment (no company/delayed payment)
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.ShippingDate = DateTime.Now.AddDays(7);

            if (!ValidateCartStock(ShoppingCartVM.ShoppingCartList, userId, out var stockError))
            {
                TempData["error"] = stockError;
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                var orderDetail = new OrderDetail
                {
                    ItemId = cart.ItemId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Item.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }

            _unitOfWork.Save();

            // Stripe payment for all orders
            var domain = $"{Request.Scheme}://{Request.Host}/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Buyer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + "Buyer/Cart/Index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in ShoppingCartVM.ShoppingCartList)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Item.Price * 100),
                        Currency = "nzd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Item.Name
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeader.UpdateStripePaymentID(
                ShoppingCartVM.OrderHeader.Id,
                session.Id,
                session.PaymentIntentId ?? string.Empty);
            _unitOfWork.Save();

            Response.Headers.Append("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult OrderConfirmation(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(
                u => u.Id == id,
                includeProperties: "ApplicationUser");

            if (orderHeader == null)
                return NotFound();

            if (orderHeader.ApplicationUserId != GetUserId())
                return Forbid();

            if (!string.IsNullOrEmpty(orderHeader.SessionId))
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower().Equals("paid", StringComparison.OrdinalIgnoreCase))
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId ?? string.Empty);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();

                    var carts = _unitOfWork.ShoppingCart
                        .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId)
                        .ToList();

                    if (carts.Count > 0)
                        FulfillOrder(id, orderHeader.ApplicationUserId);

                    HttpContext.Session.Clear();
                }
            }

            return View(id);
        }

        private void FulfillOrder(int orderHeaderId, string userId)
        {
            if (_unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId) == null)
                return;

            var details = _unitOfWork.OrderDetail
                .GetAll(d => d.OrderHeaderId == orderHeaderId)
                .ToList();

            foreach (var detail in details)
            {
                var item = _unitOfWork.Item.Get(i => i.Id == detail.ItemId, tracked: true);
                if (item == null) continue;

                item.Quantity -= detail.Count;
                if (item.Quantity <= 0)
                    item.Status = ItemStatus.SoldOut;

                _unitOfWork.Item.Update(item);
            }

            var shoppingCarts = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == userId)
                .ToList();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
        }

        private bool ValidateCartStock(IEnumerable<ShoppingCart> cartList, string userId, out string error)
        {
            foreach (var cart in cartList)
            {
                var item = cart.Item ?? _unitOfWork.Item.Get(i => i.Id == cart.ItemId);
                if (item == null || item.Status != ItemStatus.Available)
                {
                    error = $"Item \"{item?.Name ?? "Unknown"}\" is no longer available.";
                    return false;
                }

                if (item.SellerId == userId)
                {
                    error = "You cannot purchase your own listing.";
                    return false;
                }

                if (cart.Count > item.Quantity)
                {
                    error = $"Not enough stock for \"{item.Name}\".";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private ShoppingCartVM BuildCartViewModel(string userId)
        {
            var cartList = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == userId, includeProperties: "Item,Item.Category")
                .ToList();

            var vm = new ShoppingCartVM { ShoppingCartList = cartList };
            foreach (var cart in cartList)
                vm.OrderHeader.OrderTotal += cart.Price;

            return vm;
        }

        private string GetUserId()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            return claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        }
    }
}