using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ReMarket.DataAccess.Repository.IRepository;

namespace ReMarket.Web.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IViewComponentResult Invoke()
        {
            if (User.Identity?.IsAuthenticated != true)
                return View(0);

            var userId = ((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return View(0);

            var count = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == userId)
                .Sum(u => u.Count);

            return View(count);
        }
    }
}
