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
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [System.Text.Json.Serialization.JsonIgnore]
        public Product? Product { get; set; }
    }
}
