namespace ReMarket.Models.ViewModel
{
    public enum ItemImageGalleryMode
    {
        Detail,
        Card,
        Compact
    }

    public class ItemImageGalleryViewModel
    {
        public IReadOnlyList<string> ImageUrls { get; init; } = Array.Empty<string>();
        public string AltText { get; init; } = "Item";
        public string GalleryId { get; init; } = Guid.NewGuid().ToString("N")[..8];
        public ItemImageGalleryMode Mode { get; init; } = ItemImageGalleryMode.Detail;
        public int? ItemId { get; init; }
        public bool ShowOwnerDelete { get; init; }
        public string? ReturnSlug { get; init; }
        public string DeleteArea { get; init; } = "Seller";
        public string? ReturnTo { get; init; }
    }
}
