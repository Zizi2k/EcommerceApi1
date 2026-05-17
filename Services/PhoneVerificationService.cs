using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace EcommerceApi.Services
{
    /// <summary>Lưu OTP và token xác minh SĐT trong bộ nhớ (demo — không gửi SMS thật).</summary>
    public class PhoneVerificationService
    {
        private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan VerifiedLifetime = TimeSpan.FromMinutes(20);
        private const int MaxVerifyAttempts = 5;

        private readonly ConcurrentDictionary<string, OtpRecord> _otpByPhone = new();
        private readonly ConcurrentDictionary<string, VerifiedRecord> _tokenByValue = new();

        private sealed class OtpRecord
        {
            public required string Code { get; init; }
            public DateTime ExpiresUtc { get; init; }
            public int Attempts;
        }

        private sealed class VerifiedRecord
        {
            public required string Phone { get; init; }
            public required int UserId { get; init; }
            public DateTime ExpiresUtc { get; init; }
        }

        public static string NormalizePhone(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("84") && digits.Length >= 11)
                digits = "0" + digits[2..];
            return digits;
        }

        public static bool IsValidVietnamPhone(string normalized)
        {
            if (normalized.Length != 10 || normalized[0] != '0') return false;
            return normalized.All(char.IsDigit);
        }

        /// <summary>Tạo OTP 6 chữ số; trả về mã (demo hiển thị cho client).</summary>
        public string SendOtp(string phoneRaw)
        {
            var phone = NormalizePhone(phoneRaw);
            if (!IsValidVietnamPhone(phone))
                throw new ArgumentException("Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0).");

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            _otpByPhone[phone] = new OtpRecord
            {
                Code = code,
                ExpiresUtc = DateTime.UtcNow.Add(OtpLifetime),
                Attempts = 0
            };
            return code;
        }

        public string VerifyOtp(string phoneRaw, string code, int userId)
        {
            var phone = NormalizePhone(phoneRaw);
            if (!IsValidVietnamPhone(phone))
                throw new ArgumentException("Số điện thoại không hợp lệ.");

            if (string.IsNullOrWhiteSpace(code) || code.Trim().Length != 6)
                throw new ArgumentException("Mã xác nhận phải gồm 6 chữ số.");

            if (!_otpByPhone.TryGetValue(phone, out var record))
                throw new InvalidOperationException("Chưa gửi mã hoặc mã đã hết hạn. Vui lòng gửi lại.");

            if (DateTime.UtcNow > record.ExpiresUtc)
            {
                _otpByPhone.TryRemove(phone, out _);
                throw new InvalidOperationException("Mã đã hết hạn. Vui lòng gửi mã mới.");
            }

            record.Attempts++;
            if (record.Attempts > MaxVerifyAttempts)
            {
                _otpByPhone.TryRemove(phone, out _);
                throw new InvalidOperationException("Nhập sai quá nhiều lần. Vui lòng gửi mã mới.");
            }

            if (record.Code != code.Trim())
                throw new InvalidOperationException("Mã xác nhận không đúng.");

            _otpByPhone.TryRemove(phone, out _);

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            _tokenByValue[token] = new VerifiedRecord
            {
                Phone = phone,
                UserId = userId,
                ExpiresUtc = DateTime.UtcNow.Add(VerifiedLifetime)
            };
            return token;
        }

        public bool TryConsumeToken(string? token, int userId, string phoneRaw, out string normalizedPhone)
        {
            normalizedPhone = NormalizePhone(phoneRaw);
            if (string.IsNullOrWhiteSpace(token) || !IsValidVietnamPhone(normalizedPhone))
                return false;

            if (!_tokenByValue.TryGetValue(token, out var record))
                return false;

            if (DateTime.UtcNow > record.ExpiresUtc
                || record.UserId != userId
                || !string.Equals(record.Phone, normalizedPhone, StringComparison.Ordinal))
            {
                _tokenByValue.TryRemove(token, out _);
                return false;
            }

            _tokenByValue.TryRemove(token, out _);
            return true;
        }
    }
}
