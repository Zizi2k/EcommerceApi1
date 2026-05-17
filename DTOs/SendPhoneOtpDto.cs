using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.DTOs
{
    public class SendPhoneOtpDto
    {
        [Required]
        public string Phone { get; set; } = string.Empty;
    }
}
