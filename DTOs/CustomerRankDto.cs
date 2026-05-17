namespace EcommerceApi.DTOs
{
    public class CustomerOrderSummaryDto
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public int ItemCount { get; set; }
    }

    public class CustomerRankDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string AccountUsername { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? ShippingAddress { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastOrderAtUtc { get; set; }
        public string Tier { get; set; } = string.Empty;
        public string TierLabel { get; set; } = string.Empty;
        public List<CustomerOrderSummaryDto> RecentOrders { get; set; } = new();
    }
}
