namespace ReMarket.Models.ViewModel
{
    public class OrderVM
    {
        public OrderHeader OrderHeader { get; set; } = null!;
        public IEnumerable<OrderDetail> OrderDetail { get; set; } = [];
    }
}
