namespace EcommerceApi.Models
{
    /// <summary>Đơn hàng sau thanh toán (demo).</summary>
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        /// <summary>Tên đăng nhập (admin, khach, user…) lúc đặt hàng.</summary>
        public string? AccountUsername { get; set; }
        public decimal TotalAmount { get; set; }
        /// <summary>Mã: COD, BankTransfer, MoMo, VNPay, Card</summary>
        public string PaymentMethod { get; set; } = string.Empty;
        /// <summary>Preparing | Delivering | Delivered | Cancelled</summary>
        public string Status { get; set; } = OrderStatuses.Preparing;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        /// <summary>Thời điểm đơn được chuyển sang trạng thái Đã giao.</summary>
        public DateTime? DeliveredAtUtc { get; set; }

        /// <summary>Đánh giá admin (1–5), chỉ khi đã giao.</summary>
        public int? AdminRating { get; set; }
        public string? AdminReviewNote { get; set; }
        public DateTime? AdminReviewedAtUtc { get; set; }
        /// <summary>Đánh giá của khách hàng (1-5), sau khi nhận hàng.</summary>
        public int? CustomerRating { get; set; }
        public string? CustomerReviewNote { get; set; }
        public DateTime? CustomerReviewedAtUtc { get; set; }
        /// <summary>Yêu cầu hủy từ user.</summary>
        public string? CancelReason { get; set; }
        public string? CancelNote { get; set; }
        public DateTime? CancelRequestedAtUtc { get; set; }

        /// <summary>Thông tin giao hàng / COD.</summary>
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? ShippingAddress { get; set; }
        public bool PhoneVerified { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
