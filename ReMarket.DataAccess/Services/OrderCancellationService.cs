using ReMarket.DataAccess.Repository.IRepository;
using ReMarket.Models;
using ReMarket.Utility;
using Stripe;

namespace ReMarket.DataAccess.Services
{
    public static class OrderCancellationService
    {
        public static bool CanCancel(OrderHeader orderHeader)
        {
            return orderHeader.OrderStatus != SD.StatusRefunded
                && orderHeader.OrderStatus != SD.StatusCancelled
                && orderHeader.OrderStatus != SD.StatusShipped;
        }

        public static void Cancel(IUnitOfWork unitOfWork, OrderHeader orderHeader)
        {
            if (!CanCancel(orderHeader))
                throw new InvalidOperationException("This order cannot be cancelled.");

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved
                && !string.IsNullOrEmpty(orderHeader.PaymentIntentId))
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                service.Create(options);
                unitOfWork.OrderHeader.UpdateStatus(
                    orderHeader.Id,
                    SD.StatusCancelled,
                    SD.PaymentStatusRefunded);
            }
            else
            {
                unitOfWork.OrderHeader.UpdateStatus(
                    orderHeader.Id,
                    SD.StatusCancelled,
                    SD.PaymentStatusRejected);
            }

            unitOfWork.Save();
        }
    }
}
