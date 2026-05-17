using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.DTOs
{
    public class VerifyPhoneOtpDto
    {
        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;
    }
}
