namespace EcommerceApi.DTOs
{
    public class UserOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class UserOrderDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? ShippingAddress { get; set; }
        public int? CustomerRating { get; set; }
        public string? CustomerReviewNote { get; set; }
        public DateTime? CustomerReviewedAtUtc { get; set; }
        public string? CancelReason { get; set; }
        public string? CancelNote { get; set; }
        public DateTime? CancelRequestedAtUtc { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReview { get; set; }
        public List<UserOrderItemDto> Items { get; set; } = new();
    }

    public class SubmitUserOrderReviewDto
    {
        public int Rating { get; set; }
        public string? Note { get; set; }
    }

    public class SubmitOrderCancelDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonPropertyName("note")]
        public string? Note { get; set; }
    }
}
