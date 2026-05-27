namespace EcommerceApi.DTOs
{
    public class PromotionalProductCreateDto
    {
        public int ProductId { get; set; }
        public string? Headline { get; set; }
        public string? Subtitle { get; set; }
        public string? BadgeText { get; set; }
        public decimal? PromoPrice { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFlashSale { get; set; }
        public string? FlashSaleType { get; set; }
        public string? DailySlotKey { get; set; }
        public string? DailyStartTime { get; set; }
        public string? DailyEndTime { get; set; }
        public DateTime? EventStartUtc { get; set; }
        public DateTime? EventEndUtc { get; set; }
    }

    public class PromotionalProductUpdateDto : PromotionalProductCreateDto
    {
    }
}
