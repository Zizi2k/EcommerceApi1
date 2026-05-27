using EcommerceApi.Data;
using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services
{
    /// <summary>Hồ sơ: tài khoản demo (file) hoặc user Google/DB (email).</summary>
    public class UserProfileResolver
    {
        private readonly ApplicationDbContext _db;
        private readonly IDemoUserStore _demo;

        public UserProfileResolver(ApplicationDbContext db, IDemoUserStore demo)
        {
            _db = db;
            _demo = demo;
        }

        public static bool IsDbLogin(string login) =>
            !string.IsNullOrWhiteSpace(login) && login.Contains('@');

        public async Task<UserProfileData?> GetProfileAsync(string login)
        {
            var key = login.Trim();
            if (IsDbLogin(key))
            {
                var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == key);
                if (user != null) return FromDbUser(user);
            }

            if (_demo.UsernameExists(key))
                return _demo.GetProfile(key);

            return null;
        }

        public async Task<UserProfileData?> UpdateProfileAsync(string login, string? displayName, string? avatarUrl, string? backgroundUrl)
        {
            var key = login.Trim();
            if (IsDbLogin(key))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == key);
                if (user == null) return null;

                if (displayName != null)
                {
                    var n = string.IsNullOrWhiteSpace(displayName) ? key : displayName.Trim();
                    user.Name = n;
                    user.FullName = n;
                }
                if (avatarUrl != null)
                    user.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
                if (backgroundUrl != null)
                    user.BackgroundUrl = string.IsNullOrWhiteSpace(backgroundUrl) ? null : backgroundUrl.Trim();

                await _db.SaveChangesAsync();
                return FromDbUser(user);
            }

            if (!_demo.UsernameExists(key))
                return null;

            return _demo.UpdateProfile(key, displayName, avatarUrl, backgroundUrl);
        }

        public bool TryChangePassword(string login, string currentPassword, string newPassword, out string? error)
        {
            if (IsDbLogin(login))
            {
                error = "Tài khoản đăng nhập Google không đổi mật khẩu tại đây. Hãy dùng Google hoặc đăng ký tài khoản mật khẩu riêng.";
                return false;
            }
            return _demo.TryChangePassword(login, currentPassword, newPassword, out error);
        }

        private UserProfileData FromDbUser(User user)
        {
            var display = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Name;
            if (string.IsNullOrWhiteSpace(display)) display = user.Email;

            var avatar = user.AvatarUrl;
            if (string.IsNullOrWhiteSpace(avatar))
                avatar = _demo.DefaultAvatarUrl(user.Email);

            return new UserProfileData
            {
                Username = user.Email,
                DisplayName = display,
                AvatarUrl = avatar,
                BackgroundUrl = string.IsNullOrWhiteSpace(user.BackgroundUrl) ? null : user.BackgroundUrl.Trim()
            };
        }
    }
}
