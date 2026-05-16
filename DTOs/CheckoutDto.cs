using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.DTOs
{
    public class CheckoutDto
    {
        /// <summary>COD | BankTransfer | MoMo | VNPay | Card</summary>
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
