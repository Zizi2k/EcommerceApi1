namespace EcommerceApi.Configuration
{
    /// <summary>Email Google/local được cấp quyền Admin khi đăng nhập.</summary>
    public class AdminSettings
    {
        public List<string> Emails { get; set; } = new();

        public bool IsAdminEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email) || Emails.Count == 0)
                return false;

            return Emails.Any(e =>
                !string.IsNullOrWhiteSpace(e) &&
                string.Equals(e.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static AdminSettings FromConfiguration(IConfiguration configuration)
        {
            var settings = new AdminSettings();
            configuration.GetSection("Admin").Bind(settings);
            return settings;
        }
    }
}
