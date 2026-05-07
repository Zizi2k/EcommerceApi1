namespace EcommerceApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Lưu mật khẩu đã mã hóa
        public string Role { get; set; } = "Customer"; // Admin hoặc Customer
    }
}
