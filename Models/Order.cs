namespace EcommerceApi.Models
{
    /// <summary>Đơn hàng sau thanh toán (demo).</summary>
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        /// <summary>Mã: COD, BankTransfer, MoMo, VNPay, Card</summary>
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "Completed";
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
