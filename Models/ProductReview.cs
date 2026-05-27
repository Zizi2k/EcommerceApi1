namespace EcommerceApi.Models
{
    /// <summary>Đánh giá sản phẩm từ khách (một đơn đã giao, mỗi dòng hàng một bản ghi).</summary>
    public class ProductReview
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
