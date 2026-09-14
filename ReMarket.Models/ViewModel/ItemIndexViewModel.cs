namespace ReMarket.Models.ViewModel
{
    public class ItemIndexViewModel
    {
        public IEnumerable<Item> Items { get; set; } = new List<Item>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public string? Search { get; set; }
        public int? CategoryId { get; set; }

        public string? Condition { get; set; }
        public string? Location { get; set; }
        public string? DeliveryOption { get; set; }
        public string? SortBy { get; set; }
        public string? ActiveTab { get; set; }
    }
}