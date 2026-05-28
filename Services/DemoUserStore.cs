using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace EcommerceApi.Services
{
    public class DemoUserStore : IDemoUserStore
    {
        private readonly object _lock = new();
        private readonly string _filePath;
        private PersistedState _state;

        private sealed class PersistedState
        {
            public Dictionary<string, string> Passwords { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, ProfileEntry> Profiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private sealed class ProfileEntry
        {
            public string? DisplayName { get; set; }
            public string? AvatarUrl { get; set; }
            public string? BackgroundUrl { get; set; }
        }

        private static readonly Dictionary<string, string> DefaultPasswords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = "123456",
            ["khach"] = "123456",
            ["user"] = "123456",
        };

        public DemoUserStore(IWebHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "user-settings.json");
            _state = LoadOrCreate();
        }

        private static readonly Regex UsernamePattern = new(@"^[a-zA-Z0-9_]{3,32}$", RegexOptions.Compiled);
        private static readonly HashSet<string> ReservedUsernames = new(StringComparer.OrdinalIgnoreCase)
        {
            "admin", "administrator", "root", "system"
        };

        public bool UsernameExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            var key = username.Trim();
            lock (_lock)
            {
                return _state.Passwords.ContainsKey(key);
            }
        }

        public bool TryRegister(string username, string password, string? displayName, out string? error)
        {
            error = null;
            var key = username?.Trim() ?? "";
            if (!UsernamePattern.IsMatch(key))
            {
                error = "Tên đăng nhập: 3–32 ký tự, chỉ chữ, số và dấu gạch dưới (_).";
                return false;
            }
            if (ReservedUsernames.Contains(key))
            {
                error = "Tên đăng nhập này không được sử dụng.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            {
                error = "Mật khẩu phải có ít nhất 4 ký tự.";
                return false;
            }

            lock (_lock)
            {
                if (_state.Passwords.ContainsKey(key))
                {
                    error = "Tên đăng nhập đã tồn tại.";
                    return false;
                }

                _state.Passwords[key] = password;
                var display = string.IsNullOrWhiteSpace(displayName) ? key : displayName.Trim();
                _state.Profiles[key] = new ProfileEntry
                {
                    DisplayName = display,
                    AvatarUrl = DefaultAvatarUrl(key)
                };
                SaveLocked();
            }
            return true;
        }

        public bool ValidatePassword(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            var key = username.Trim();
            lock (_lock)
            {
                return _state.Passwords.TryGetValue(key, out var expected) && expected == password;
            }
        }

        public bool TryChangePassword(string username, string currentPassword, string newPassword, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(username))
            {
                error = "Thiếu tên đăng nhập.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
            {
                error = "Mật khẩu mới phải có ít nhất 4 ký tự.";
                return false;
            }

            var key = username.Trim();
            lock (_lock)
            {
                if (!_state.Passwords.TryGetValue(key, out var expected) || expected != currentPassword)
                {
                    error = "Mật khẩu hiện tại không đúng.";
                    return false;
                }
                _state.Passwords[key] = newPassword;
                SaveLocked();
            }
            return true;
        }

        public UserProfileData GetProfile(string username)
        {
            var key = NormalizeUsername(username);
            lock (_lock)
            {
                return BuildProfileLocked(key);
            }
        }

        public UserProfileData UpdateProfile(string username, string? displayName, string? avatarUrl, string? backgroundUrl)
        {
            var key = NormalizeUsername(username);
            lock (_lock)
            {
                EnsureUserExistsLocked(key);
                if (!_state.Profiles.TryGetValue(key, out var entry))
                {
                    entry = new ProfileEntry();
                    _state.Profiles[key] = entry;
                }

                if (displayName != null)
                    entry.DisplayName = string.IsNullOrWhiteSpace(displayName) ? key : displayName.Trim();
                if (avatarUrl != null)
                    entry.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
                if (backgroundUrl != null)
                    entry.BackgroundUrl = string.IsNullOrWhiteSpace(backgroundUrl) ? null : backgroundUrl.Trim();

                SaveLocked();
                return BuildProfileLocked(key);
            }
        }

        public string DefaultAvatarUrl(string username)
        {
            var seed = string.IsNullOrWhiteSpace(username) ? "guest" : username.Trim();
            return $"https://api.dicebear.com/7.x/avataaars/svg?seed={Uri.EscapeDataString(seed)}";
        }

        public string NameIdentifierFor(string username)
        {
            if (string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase)) return "1";
            if (string.Equals(username, "khach", StringComparison.OrdinalIgnoreCase)) return "2";
            if (string.Equals(username, "user", StringComparison.OrdinalIgnoreCase)) return "3";
            var normalized = (username ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(normalized)) return "0";

            // Stable ID across app restarts (GetHashCode is process-randomized in .NET).
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            var value = BitConverter.ToUInt32(bytes, 0);
            return ((value % 999_000u) + 1000u).ToString();
        }

        public string? UsernameForUserId(int userId) => userId switch
        {
            1 => "admin",
            2 => "khach",
            3 => "user",
            _ => null
        };

        private static string NormalizeUsername(string username) =>
            string.IsNullOrWhiteSpace(username) ? "guest" : username.Trim();

        private void EnsureUserExistsLocked(string key)
        {
            if (!_state.Passwords.ContainsKey(key))
                throw new InvalidOperationException("Tài khoản không tồn tại.");
        }

        private UserProfileData BuildProfileLocked(string key)
        {
            EnsureUserExistsLocked(key);
            _state.Profiles.TryGetValue(key, out var entry);
            var display = entry?.DisplayName;
            if (string.IsNullOrWhiteSpace(display)) display = key;

            var avatar = entry?.AvatarUrl;
            if (string.IsNullOrWhiteSpace(avatar)) avatar = DefaultAvatarUrl(key);

            return new UserProfileData
            {
                Username = key,
                DisplayName = display,
                AvatarUrl = avatar,
                BackgroundUrl = string.IsNullOrWhiteSpace(entry?.BackgroundUrl) ? null : entry!.BackgroundUrl
            };
        }

        private PersistedState LoadOrCreate()
        {
            lock (_lock)
            {
                if (File.Exists(_filePath))
                {
                    try
                    {
                        var json = File.ReadAllText(_filePath);
                        var loaded = JsonSerializer.Deserialize<PersistedState>(json);
                        if (loaded != null)
                        {
                            loaded.Passwords = new Dictionary<string, string>(loaded.Passwords, StringComparer.OrdinalIgnoreCase);
                            loaded.Profiles = new Dictionary<string, ProfileEntry>(loaded.Profiles, StringComparer.OrdinalIgnoreCase);
                            foreach (var kv in DefaultPasswords)
                            {
                                if (!loaded.Passwords.ContainsKey(kv.Key))
                                    loaded.Passwords[kv.Key] = kv.Value;
                            }
                            _state = loaded;
                            return loaded;
                        }
                    }
                    catch
                    {
                        /* fall through to defaults */
                    }
                }

                _state = new PersistedState
                {
                    Passwords = new Dictionary<string, string>(DefaultPasswords, StringComparer.OrdinalIgnoreCase),
                    Profiles = new Dictionary<string, ProfileEntry>(StringComparer.OrdinalIgnoreCase)
                };
                SaveLocked();
                return _state;
            }
        }

        private void SaveLocked()
        {
            var json = JsonSerializer.Serialize(_state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}
