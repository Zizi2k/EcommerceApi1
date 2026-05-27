namespace EcommerceApi.DTOs
{
    public class ProductReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class ProductReviewSummaryDto
    {
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
        public List<ProductReviewDto> Reviews { get; set; } = new();
    }
}
