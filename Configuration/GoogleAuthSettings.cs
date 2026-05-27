namespace EcommerceApi.Configuration
{
    public class GoogleAuthSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(ClientId)
            && !string.IsNullOrWhiteSpace(ClientSecret)
            && !ClientId.Contains("YOUR_GOOGLE", StringComparison.OrdinalIgnoreCase)
            && !ClientSecret.Contains("YOUR_GOOGLE", StringComparison.OrdinalIgnoreCase);

        public static GoogleAuthSettings FromConfiguration(IConfiguration configuration)
        {
            var settings = new GoogleAuthSettings();
            configuration.GetSection("Authentication:Google").Bind(settings);
            if (string.IsNullOrWhiteSpace(settings.ClientId))
                settings.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "";
            if (string.IsNullOrWhiteSpace(settings.ClientSecret))
                settings.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "";
            return settings;
        }
    }
}
