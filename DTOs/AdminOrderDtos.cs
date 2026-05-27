namespace EcommerceApi.DTOs
{
    public class AdminOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class AdminOrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? AccountUsername { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public int? AdminRating { get; set; }
        public string? AdminReviewNote { get; set; }
        public DateTime? AdminReviewedAtUtc { get; set; }
        public int? CustomerRating { get; set; }
        public string? CustomerReviewNote { get; set; }
        public DateTime? CustomerReviewedAtUtc { get; set; }
        public string? CancelReason { get; set; }
        public string? CancelNote { get; set; }
        public DateTime? CancelRequestedAtUtc { get; set; }
        public bool HasCancelRequest { get; set; }
        /// <summary>Admin có thể chấp nhận/từ chối khi có yêu cầu hủy (chưa giao / chưa hủy).</summary>
        public bool CanRespondToCancelRequest { get; set; }
        public bool CanReview { get; set; }
        public List<AdminOrderItemDto> Items { get; set; } = new();
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class SubmitOrderReviewDto
    {
        public int Rating { get; set; }
        public string? Note { get; set; }
    }
}
