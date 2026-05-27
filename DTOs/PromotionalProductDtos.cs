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
    }

    public class PromotionalProductUpdateDto : PromotionalProductCreateDto
    {
    }
}
