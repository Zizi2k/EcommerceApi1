namespace EcommerceApi.DTOs
{
    public class AdminMonthlyProductReportRowDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
        public decimal Capital { get; set; }
        public decimal Profit { get; set; }
    }

    public class AdminMonthlyOrderReportDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;
        public int DeliveredOrderCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCapital { get; set; }
        public decimal TotalProfit { get; set; }
        public List<AdminMonthlyProductReportRowDto> Products { get; set; } = new();
    }
}
