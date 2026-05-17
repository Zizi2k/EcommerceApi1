namespace EcommerceApi.Services
{
    public sealed class UserProfileData
    {
        public string Username { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string AvatarUrl { get; init; } = string.Empty;
        public string? BackgroundUrl { get; init; }
    }

    public interface IDemoUserStore
    {
        bool ValidatePassword(string username, string password);
        bool TryRegister(string username, string password, string? displayName, out string? error);
        bool UsernameExists(string username);
        bool TryChangePassword(string username, string currentPassword, string newPassword, out string? error);
        UserProfileData GetProfile(string username);
        UserProfileData UpdateProfile(string username, string? displayName, string? avatarUrl, string? backgroundUrl);
        string DefaultAvatarUrl(string username);
        string NameIdentifierFor(string username);
        string? UsernameForUserId(int userId);
    }
}
