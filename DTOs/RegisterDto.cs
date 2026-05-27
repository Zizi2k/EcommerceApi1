namespace EcommerceApi.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? ConfirmPassword { get; set; }
        public string? DisplayName { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string OtpToken { get; set; } = string.Empty;
    }
}
