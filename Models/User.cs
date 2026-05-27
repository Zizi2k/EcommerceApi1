namespace EcommerceApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        /// <summary>Tên hiển thị (cột Name trong DB).</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Họ tên đầy đủ (cột FullName trong DB — bắt buộc).</summary>
        public string FullName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
        public string AuthProvider { get; set; } = "Local";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? GoogleSub { get; set; }
        public string? AvatarUrl { get; set; }
        public string? GoogleId { get; set; }
    }
}
