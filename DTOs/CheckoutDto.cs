using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.DTOs
{
    public class CheckoutDto
    {
        /// <summary>COD | BankTransfer | MoMo | VNPay | Card</summary>
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>Bắt buộc khi thanh toán COD (tiền mặt).</summary>
        public string? CustomerName { get; set; }

        public string? CustomerPhone { get; set; }

        public string? ShippingAddress { get; set; }
    }
}
