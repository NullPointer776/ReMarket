namespace ReMarket.Models.ViewModel
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; } = [];
        public OrderHeader OrderHeader { get; set; } = new();
    }
}
