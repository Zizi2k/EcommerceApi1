namespace EcommerceApi.Models
{
    public class PromotionalProduct
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Headline { get; set; }
        public string? Subtitle { get; set; }
        public string? BadgeText { get; set; }
        public decimal? PromoPrice { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFlashSale { get; set; }
        /// <summary>None | DailySlot | Event</summary>
        public string FlashSaleType { get; set; } = "None";
        /// <summary>MORNING | NOON | EVENING (chỉ dùng khi DailySlot).</summary>
        public string? DailySlotKey { get; set; }
        /// <summary>Phút trong ngày (0..1439), cho phép admin tự chỉnh khung giờ hằng ngày.</summary>
        public int? DailyStartMinute { get; set; }
        public int? DailyEndMinute { get; set; }
        public DateTime? EventStartUtc { get; set; }
        public DateTime? EventEndUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [System.Text.Json.Serialization.JsonIgnore]
        public Product? Product { get; set; }
    }
}
